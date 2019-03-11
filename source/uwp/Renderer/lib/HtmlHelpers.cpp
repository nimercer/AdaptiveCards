#include "pch.h"
#include "XamlHelpers.h"
#include "HtmlHelpers.h"
#include <safeint.h>

using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace ABI::AdaptiveNamespace;
using namespace ABI::Windows::Foundation;
using namespace ABI::Windows::Foundation::Collections;
using namespace AdaptiveNamespace;
using namespace msl::utilities;
using namespace ABI::Windows::UI::Xaml::Media;
using namespace ABI::Windows::UI::Xaml;

ComPtr<IBrush> GetSolidColorBrush(ABI::Windows::UI::Color color)
{
    ComPtr<ISolidColorBrush> solidColorBrush =
        XamlHelpers::CreateXamlClass<ISolidColorBrush>(HStringReference(RuntimeClass_Windows_UI_Xaml_Media_SolidColorBrush));
    THROW_IF_FAILED(solidColorBrush->put_Color(color));
    ComPtr<IBrush> solidColorBrushAsBrush;
    THROW_IF_FAILED(solidColorBrush.As(&solidColorBrushAsBrush));
    return solidColorBrushAsBrush;
}

template<typename T>
void StyleInlineProperties(ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                           ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                           ABI::AdaptiveNamespace::FontStyle fontStyle,
                           ABI::AdaptiveNamespace::TextSize size,
                           ABI::AdaptiveNamespace::ForegroundColor color,
                           bool isSubtle,
                           UINT32 maxWidth,
                           ABI::AdaptiveNamespace::TextWeight weight,
                           _In_ T* xamlTextBlock)
{
    ComPtr<T> localTextBlock(xamlTextBlock);

    ABI::AdaptiveNamespace::ContainerStyle containerStyle;
    renderArgs->get_ContainerStyle(&containerStyle);

    ComPtr<IAdaptiveHostConfig> hostConfig;
    renderContext->get_HostConfig(&hostConfig);

    ABI::Windows::UI::Color fontColor;
    THROW_IF_FAILED(GetColorFromAdaptiveColor(hostConfig.Get(), color, containerStyle, isSubtle, &fontColor));

    ComPtr<IBrush> fontColorBrush = GetSolidColorBrush(fontColor);
    THROW_IF_FAILED(localTextBlock->put_Foreground(fontColorBrush.Get()));

    HString fontFamilyName;
    UINT32 fontSize;
    ABI::Windows::UI::Text::FontWeight xamlFontWeight;

    // Retrieve the desired FontFamily, FontSize, and FontWeight values
    THROW_IF_FAILED(GetFontDataFromStyle(hostConfig.Get(), fontStyle, size, weight, fontFamilyName.GetAddressOf(), &fontSize, &xamlFontWeight));

    // Apply font size
    THROW_IF_FAILED(localTextBlock->put_FontSize((double)fontSize));

    // Apply font weight
    THROW_IF_FAILED(localTextBlock->put_FontWeight(xamlFontWeight));

    // Apply font family
    ComPtr<IInspectable> inspectable;
    ComPtr<IFontFamily> fontFamily;
    ComPtr<IFontFamilyFactory> fontFamilyFactory;
    THROW_IF_FAILED(Windows::Foundation::GetActivationFactory(HStringReference(L"Windows.UI.Xaml.Media.FontFamily").Get(),
                                                              &fontFamilyFactory));
    THROW_IF_FAILED(
        fontFamilyFactory->CreateInstanceWithName(fontFamilyName.Get(), nullptr, inspectable.ReleaseAndGetAddressOf(), &fontFamily));
    THROW_IF_FAILED(xamlTextBlock->put_FontFamily(fontFamily.Get()));

    if (maxWidth != MAXUINT32)
    {
        ComPtr<IFrameworkElement> textBlockAsFrameworkElement;
        THROW_IF_FAILED(localTextBlock.As(&textBlockAsFrameworkElement));
        THROW_IF_FAILED(textBlockAsFrameworkElement->put_MaxWidth(maxWidth));
    }
}

template<typename T>
HRESULT StyleInlinePropertiesFromTextElement(_In_ IAdaptiveTextElement* adaptiveTextElement,
                                             _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                                             _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                                             _In_ T* elementToStyle)
{
    ABI::AdaptiveNamespace::FontStyle fontStyle;
    RETURN_IF_FAILED(adaptiveTextElement->get_FontStyle(&fontStyle));

    ABI::AdaptiveNamespace::ForegroundColor textColor;
    RETURN_IF_FAILED(adaptiveTextElement->get_Color(&textColor));
    boolean isSubtle = false;
    RETURN_IF_FAILED(adaptiveTextElement->get_IsSubtle(&isSubtle));

    ABI::AdaptiveNamespace::TextSize textblockSize;
    RETURN_IF_FAILED(adaptiveTextElement->get_Size(&textblockSize));

    ABI::AdaptiveNamespace::TextWeight textWeight;
    RETURN_IF_FAILED(adaptiveTextElement->get_Weight(&textWeight));

    StyleInlineProperties(renderContext, renderArgs, fontStyle, textblockSize, textColor, isSubtle, MAXUINT32, textWeight, elementToStyle);

    return S_OK;
}

static HRESULT GetTextFromXmlNode(_In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node, _In_ HSTRING* text)
{
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> localNode = node;
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNodeSerializer> textNodeSerializer;
    RETURN_IF_FAILED(localNode.As(&textNodeSerializer));

    RETURN_IF_FAILED(textNodeSerializer->get_InnerText(text));
    return S_OK;
}

HRESULT AddListInlines(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                       _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                       bool isListOrdered,
                       _In_ IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines)
{
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNamedNodeMap> attributeMap;
    RETURN_IF_FAILED(node->get_Attributes(&attributeMap));

    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> startNode;
    RETURN_IF_FAILED(attributeMap->GetNamedItem(HStringReference(L"start").Get(), &startNode));

    unsigned long iteration = 1;
    if (startNode != nullptr)
    {
        // Get the starting value for this list
        HString start;
        RETURN_IF_FAILED(GetTextFromXmlNode(startNode.Get(), start.GetAddressOf()));
        try
        {
            iteration = std::stoul(HStringToUTF8(start.Get()));

            // Check that we can iterate the entire list without overflowing.
            // If the list values are too big to store in an unsigned int, start
            // the list at 1.
            ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNodeList> children;
            RETURN_IF_FAILED(node->get_ChildNodes(&children));

            UINT32 childrenLength;
            RETURN_IF_FAILED(children->get_Length(&childrenLength));

            unsigned long result;
            if (!SafeAdd(iteration, childrenLength - 1, result))
            {
                iteration = 1;
            }
        }
        catch (const std::out_of_range&)
        {
            // If stoul throws out_of_range, start the numbered list at 1.
        }
    }

    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> listChild;
    RETURN_IF_FAILED(node->get_FirstChild(&listChild));

    while (listChild != nullptr)
    {
        std::wstring listElementString = L"\n";
        if (!isListOrdered)
        {
            // Add a bullet
            listElementString += L"\x2022 ";
        }
        else
        {
            listElementString += std::to_wstring(iteration);
            listElementString += L". ";
        }

        HString listElementHString;
        RETURN_IF_FAILED(listElementHString.Set(listElementString.c_str()));

        ComPtr<ABI::Windows::UI::Xaml::Documents::IRun> run =
            XamlHelpers::CreateXamlClass<ABI::Windows::UI::Xaml::Documents::IRun>(
                HStringReference(RuntimeClass_Windows_UI_Xaml_Documents_Run));
        RETURN_IF_FAILED(run->put_Text(listElementHString.Get()));

        ComPtr<ABI::Windows::UI::Xaml::Documents::IInline> runAsInline;
        RETURN_IF_FAILED(run.As(&runAsInline));

        RETURN_IF_FAILED(inlines->Append(runAsInline.Get()));

        RETURN_IF_FAILED(AddTextInlines(adaptiveTextElement, renderContext, renderArgs, listChild.Get(), false, false, inlines));

        ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> nextListChild;
        RETURN_IF_FAILED(listChild->get_NextSibling(&nextListChild));

        iteration++;
        listChild = nextListChild;
    }
    return S_OK;
}

HRESULT AddLinkInline(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                      _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                      _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                      _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                      BOOL isBold,
                      BOOL isItalic,
                      _In_ IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines)
{
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNamedNodeMap> attributeMap;
    RETURN_IF_FAILED(node->get_Attributes(&attributeMap));

    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> hrefNode;
    RETURN_IF_FAILED(attributeMap->GetNamedItem(HStringReference(L"href").Get(), &hrefNode));

    if (hrefNode == nullptr)
    {
        return E_INVALIDARG;
    }

    HString href;
    RETURN_IF_FAILED(GetTextFromXmlNode(hrefNode.Get(), href.GetAddressOf()));

    ComPtr<IUriRuntimeClassFactory> uriActivationFactory;
    RETURN_IF_FAILED(GetActivationFactory(HStringReference(RuntimeClass_Windows_Foundation_Uri).Get(), &uriActivationFactory));

    ComPtr<IUriRuntimeClass> uri;
    RETURN_IF_FAILED(uriActivationFactory->CreateUri(href.Get(), uri.GetAddressOf()));

    ComPtr<ABI::Windows::UI::Xaml::Documents::IHyperlink> hyperlink =
        XamlHelpers::CreateXamlClass<ABI::Windows::UI::Xaml::Documents::IHyperlink>(
            HStringReference(RuntimeClass_Windows_UI_Xaml_Documents_Hyperlink));
    RETURN_IF_FAILED(hyperlink->put_NavigateUri(uri.Get()));

    ComPtr<ABI::Windows::UI::Xaml::Documents::ISpan> hyperlinkAsSpan;
    RETURN_IF_FAILED(hyperlink.As(&hyperlinkAsSpan));

    ComPtr<IVector<ABI::Windows::UI::Xaml::Documents::Inline*>> hyperlinkInlines;
    RETURN_IF_FAILED(hyperlinkAsSpan->get_Inlines(hyperlinkInlines.GetAddressOf()));

    RETURN_IF_FAILED(
        AddTextInlines(adaptiveTextElement, renderContext, renderArgs, node, isBold, isItalic, hyperlinkInlines.Get()));

    ComPtr<ABI::Windows::UI::Xaml::Documents::IInline> hyperLinkAsInline;
    RETURN_IF_FAILED(hyperlink.As(&hyperLinkAsInline));
    RETURN_IF_FAILED(inlines->Append(hyperLinkAsInline.Get()));

    return S_OK;
}

HRESULT AddSingleTextInline(_In_ IAdaptiveTextElement* adaptiveTextElement,
                            _In_ IAdaptiveRenderContext* renderContext,
                            _In_ IAdaptiveRenderArgs* renderArgs,
                            _In_ HSTRING string,
                            bool isBold,
                            bool isItalic,
                            _In_ IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines)
{
    ComPtr<ABI::Windows::UI::Xaml::Documents::IRun> run = XamlHelpers::CreateXamlClass<ABI::Windows::UI::Xaml::Documents::IRun>(
        HStringReference(RuntimeClass_Windows_UI_Xaml_Documents_Run));
    RETURN_IF_FAILED(run->put_Text(string));

    ComPtr<ABI::Windows::UI::Xaml::Documents::ITextElement> runAsTextElement;
    RETURN_IF_FAILED(run.As(&runAsTextElement));

    RETURN_IF_FAILED(
        StyleInlinePropertiesFromTextElement(adaptiveTextElement, renderContext, renderArgs, runAsTextElement.Get()));

    if (isBold)
    {
        ComPtr<IAdaptiveHostConfig> hostConfig;
        RETURN_IF_FAILED(renderContext->get_HostConfig(&hostConfig));

        ABI::AdaptiveNamespace::FontStyle fontStyle;
        RETURN_IF_FAILED(adaptiveTextElement->get_FontStyle(&fontStyle));

        ABI::Windows::UI::Text::FontWeight boldFontWeight;
        RETURN_IF_FAILED(GetFontWeightFromStyle(hostConfig.Get(), fontStyle, ABI::AdaptiveNamespace::TextWeight::Bolder, &boldFontWeight));

        RETURN_IF_FAILED(runAsTextElement->put_FontWeight(boldFontWeight));
    }

    if (isItalic)
    {
        RETURN_IF_FAILED(runAsTextElement->put_FontStyle(ABI::Windows::UI::Text::FontStyle::FontStyle_Italic));
    }

    ComPtr<ABI::Windows::UI::Xaml::Documents::IInline> runAsInline;
    RETURN_IF_FAILED(run.As(&runAsInline));

    RETURN_IF_FAILED(inlines->Append(runAsInline.Get()));
    return S_OK;
}

HRESULT AddTextInlines(_In_ IAdaptiveTextElement* adaptiveTextElement,
                       _In_ IAdaptiveRenderContext* renderContext,
                       _In_ IAdaptiveRenderArgs* renderArgs,
                       _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                       BOOL isBold,
                       BOOL isItalic,
                       _In_ IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines)
{
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> childNode;
    RETURN_IF_FAILED(node->get_FirstChild(&childNode));

    while (childNode != nullptr)
    {
        HString nodeName;
        RETURN_IF_FAILED(childNode->get_NodeName(nodeName.GetAddressOf()));

        INT32 isLinkResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"a").Get(), &isLinkResult));

        INT32 isBoldResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"strong").Get(), &isBoldResult));

        INT32 isItalicResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"em").Get(), &isItalicResult));

        INT32 isTextResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"#text").Get(), &isTextResult));

        if (isLinkResult == 0)
        {
            RETURN_IF_FAILED(AddLinkInline(adaptiveTextElement, renderContext, renderArgs, childNode.Get(), isBold, isItalic, inlines));
        }
        else if (isTextResult == 0)
        {
            HString text;
            RETURN_IF_FAILED(GetTextFromXmlNode(childNode.Get(), text.GetAddressOf()));
            RETURN_IF_FAILED(AddSingleTextInline(adaptiveTextElement, renderContext, renderArgs, text.Get(), isBold, isItalic, inlines));
        }
        else
        {
            RETURN_IF_FAILED(AddTextInlines(adaptiveTextElement,
                                            renderContext,
                                            renderArgs,
                                            childNode.Get(),
                                            isBold || (isBoldResult == 0),
                                            isItalic || (isItalicResult == 0),
                                            inlines));
        }

        ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> nextChildNode;
        RETURN_IF_FAILED(childNode->get_NextSibling(&nextChildNode));
        childNode = nextChildNode;
    }

    return S_OK;
}

HRESULT AddHtmlInlines(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                       _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                       _In_ IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines)
{
    ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> childNode;
    RETURN_IF_FAILED(node->get_FirstChild(&childNode));

    while (childNode != nullptr)
    {
        HString nodeName;
        RETURN_IF_FAILED(childNode->get_NodeName(nodeName.GetAddressOf()));

        INT32 isOrderedListResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"ol").Get(), &isOrderedListResult));

        INT32 isUnorderedListResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"ul").Get(), &isUnorderedListResult));

        INT32 isParagraphResult;
        RETURN_IF_FAILED(WindowsCompareStringOrdinal(nodeName.Get(), HStringReference(L"p").Get(), &isParagraphResult));

        if ((isOrderedListResult == 0) || (isUnorderedListResult == 0))
        {
            RETURN_IF_FAILED(
                AddListInlines(adaptiveTextElement, renderContext, renderArgs, childNode.Get(), (isOrderedListResult == 0), inlines));
        }
        else if (isParagraphResult == 0)
        {
            RETURN_IF_FAILED(AddTextInlines(adaptiveTextElement, renderContext, renderArgs, childNode.Get(), false, false, inlines));
        }
        else
        {
            RETURN_IF_FAILED(AddHtmlInlines(adaptiveTextElement, renderContext, renderArgs, childNode.Get(), inlines));
        }

        ComPtr<ABI::Windows::Data::Xml::Dom::IXmlNode> nextChildNode;
        RETURN_IF_FAILED(childNode->get_NextSibling(&nextChildNode));
        childNode = nextChildNode;
    }
    return S_OK;
}
