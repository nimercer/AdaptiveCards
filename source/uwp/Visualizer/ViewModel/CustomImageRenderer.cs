// -----------------------------------------------------------------------
// <copyright file="CustomImageRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AdaptiveCardVisualizer.ViewModel
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AdaptiveCards.Rendering.Uwp;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;

    /// <summary>
    /// Custom adaptive cards image renderer which can handle SVGs.
    /// </summary>
    public class CustomImageRenderer : IAdaptiveElementRenderer
    {
        private const int Base64Index = 3;
        private const string Base64GroupName = "base64";
        private const int LruCacheSize = 16;

        private readonly AdaptiveImageRenderer baseRenderer = new AdaptiveImageRenderer();

        // Data URI scheme syntax is data:[<media type>[;charset=<encoding>]][;base64],<data>
        private readonly Regex svgDataUriRegex = new Regex(@"data:image\/svg\+xml(;(?<params>[\w\=\-]+))?(;(?<base64>\bbase64\b))?,(?<data>.*)", RegexOptions.Compiled);

        /// <summary>
        /// LRU cache of previously loaded SvgImageSource objects.
        /// This allows us to greatly reduce the memory used when we require more than one instance of the same SVG.
        /// NOTE: The LruCache is not thread-safe, but this is OK because Render only ever gets called from the UI thread.
        /// </summary>
        //private readonly LruCache<string, WeakReference<SvgImageSource>> svgSourceCache = new LruCache<string, WeakReference<SvgImageSource>>(LruCacheSize);

        /// <summary>
        /// Creates a UI element which is a representation of the adaptive card element specified.
        /// </summary>
        /// <param name="element">The adaptive card element to render.</param>
        /// <param name="context">The render context.</param>
        /// <param name="renderArgs">The render arguments.</param>
        /// <returns>Image UI element.</returns>
        public UIElement Render(IAdaptiveCardElement element, AdaptiveRenderContext context, AdaptiveRenderArgs renderArgs)
        {   
            // Create rendered element using the underlying renderer.
            var uielement = this.baseRenderer.Render(element, context, renderArgs);
            
            // Get the XAML Image component in question.
            // Sometimes we're actually dealing with a Button with an Image as its Content so check for that if necessary.
            var image = uielement as Image;
            if (image == null)
            {
                var buttonElement = uielement as Button;
                image = buttonElement?.Content as Image;
            }

            var adaptiveImage = element as AdaptiveImage;

            // If the image is an SVG, load it. Otherwise display as normal.
            if (image != null &&
                adaptiveImage?.Url != null &&
                adaptiveImage.Url.StartsWith("data:image/svg+xml", StringComparison.OrdinalIgnoreCase))
            {
                var match = this.svgDataUriRegex.Match(adaptiveImage.Url);

                if (match.Success)
                {
                    // Check to see if we already have a cached SvgImageSource for the SVG we're loading
                    //if (this.svgSourceCache.TryGetValue(adaptiveImage.Url, out var weakSvgSource))
                    //{
                    //    if (weakSvgSource.TryGetTarget(out var strongSvgSource))
                    //    {
                    //        // We found an existing matching SVG image source in the cache!
                    //        // Set it as the source for our image and return.
                    //        image.Source = strongSvgSource;
                    //        return uielement;
                    //    }
                    //}

                    // Create a new SVG image source from the data encoded in the URI.
                    var isBase64Encoded = match.Groups[Base64Index].Value == Base64GroupName;
                    var svgImageSource = CreateSvgImageSource(match.Groups["data"].Value, isBase64Encoded, image.MaxWidth * 2.0, image.MaxHeight * 2.0); // * 2.0 as it makes it look much sharper... ¯\_(ツ)_/¯

                    // Set the image source to be our newly created SVG source.
                    image.Source = svgImageSource;

                    // Add the created SvgImageSource to the cache so that we can use if for future image loads.
                    //this.svgSourceCache.AddOrReplace(adaptiveImage.Url, new WeakReference<SvgImageSource>(svgImageSource));
                }
            }
            else if (image != null)
            {
                /* Workaround for bug 23191024 - Sometimes image queries result in no image being displayed.
                 * https://microsoft.visualstudio.com/OS/_workitems/edit/23191024
                 * The ultimate problem is that sometime we get an image URL from Bing does doesn't resolve at first attempt but does a fraction of a second later.
                 * A specific bug has been reported to Bing and can be found here:
                 * https://msasg.visualstudio.com/Cortana/_workitems/edit/2032671
                 * For now, when we detect an image failing to load we can try to load it again but bypassing the cache (as the cache will contain the empty failed version :/).
                 */

                // If this is a image then add event handlers for when the image either successfully opens or fails.
                // In the failure case we will create a new bitmap image with the same source URI that bypasses the cache.
                // This is not recursive so if the second attempt to load the image fails then we will not attempt again.
                // The success callback is just so that we can unsubscribe the handlers cleanly.
                var bitmapImage = image.Source as BitmapImage;
                if (bitmapImage != null)
                {
                    image.ImageFailed += ImageFailed;
                    image.ImageOpened += ImageOpened;
                }
            }

            return image;
        }

        /// <summary>
        /// Image failed to load callback.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private static void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // If we're here then the image did not load correctly first time.
            // In case this is because of dodgy Bin URIs then let's retry the image load but bypass the cache this time in order to force another load from the network.
            var image = sender as Image;
            if (image != null)
            {
                var newBitmapImageSource = new BitmapImage();
                newBitmapImageSource.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                newBitmapImageSource.UriSource = (image.Source as BitmapImage)?.UriSource;
                image.Source = newBitmapImageSource;

                // Unsubscribe the callbacks as we only want to do this fallback path once.
                image.ImageFailed -= ImageFailed;
                image.ImageOpened -= ImageOpened;
            }
        }

        /// <summary>
        /// Image opened successfully callback.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private static void ImageOpened(object sender, RoutedEventArgs e)
        {
            // Clean up the callbacks correctly.
            var image = sender as Image;
            if (image != null)
            {
                image.ImageFailed -= ImageFailed;
                image.ImageOpened -= ImageOpened;
            }
        }

        /// <summary>
        /// Helper function to create an SVG image source object from the supplied encoded URI.
        /// </summary>
        /// <param name="data">The encoded URI data to create the SVG image source from.</param>
        /// <param name="isBase64Encoded">Is the data base64 encoded or not?.</param>
        /// <param name="maxWidth">Target maximum width for the SVG.</param>
        /// <param name="maxHeight">Target maximum height for the SVG.</param>
        /// <returns>The created SVG image source.</returns>
        private static SvgImageSource CreateSvgImageSource(string data, bool isBase64Encoded, double maxWidth, double maxHeight)
        {
            byte[] bytes = null;

            // If the URL is base 64 encoded, we need to parse it separately first.
            if (isBase64Encoded)
            {
                bytes = Convert.FromBase64String(data);
            }
            else
            {
                var src = WebUtility.UrlDecode(data);
                bytes = Encoding.ASCII.GetBytes(src);
            }

            var memoryStream = new MemoryStream(bytes);
            var svgImageSource = new SvgImageSource();

            // If the SvgImageSource does not have a size set then infer one from the parent image.
            // If we don't set a sensible size then the default is to render it at 'maximum window dimensions'
            // whatever that means... It essentially makes the image MASSIVE and uses silly amounts of memory.
            if (maxWidth > 0)
            {
                svgImageSource.RasterizePixelWidth = maxWidth;
            }

            if (maxHeight > 0)
            {
                svgImageSource.RasterizePixelHeight = maxHeight;
            }

            // Kick off the async setting of the image source
            _ = SetSvgSourceAsync(svgImageSource, memoryStream);

            return svgImageSource;
        }

        /// <summary>
        /// Sets the streaming data source for an SvgImageSource, waits for the async op to complete, then cleans up resources.
        /// </summary>
        /// <param name="svgImageSource">SVG image source to set the data source for.</param>
        /// <param name="memoryStream">Streaming data source containing the SVG data.</param>
        /// <returns>Task.</returns>
        private static async Task SetSvgSourceAsync(SvgImageSource svgImageSource, MemoryStream memoryStream)
        {
            try
            {
                await svgImageSource.SetSourceAsync(memoryStream.AsRandomAccessStream());
            }
            finally
            {
                // Always dispose of the memory stream
                memoryStream.Dispose();
            }
        }
    }
}
