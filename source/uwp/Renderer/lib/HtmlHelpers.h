#pragma once

HRESULT AddHtmlInlines(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                       _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                       bool isInHyperlink,
                       _In_ ABI::Windows::Foundation::Collections::IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines);

HRESULT AddTextInlines(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                       _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                       _In_ ABI::Windows::Data::Xml::Dom::IXmlNode* node,
                       BOOL isBold,
                       BOOL isItalic,
                       bool isInHyperlink,
                       _In_ ABI::Windows::Foundation::Collections::IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines);

HRESULT AddSingleTextInline(_In_ ABI::AdaptiveNamespace::IAdaptiveTextElement* adaptiveTextElement,
                            _In_ ABI::AdaptiveNamespace::IAdaptiveRenderContext* renderContext,
                            _In_ ABI::AdaptiveNamespace::IAdaptiveRenderArgs* renderArgs,
                            _In_ HSTRING string,
                            bool isBold,
                            bool isItalic,
                            bool isInHyperlink,
                            _In_ ABI::Windows::Foundation::Collections::IVector<ABI::Windows::UI::Xaml::Documents::Inline*>* inlines);

HRESULT SetMaxLines(ABI::Windows::UI::Xaml::Controls::ITextBlock* textBlock, UINT maxLines);
HRESULT SetMaxLines(ABI::Windows::UI::Xaml::Controls::IRichTextBlock* textBlock, UINT maxLines);

template<typename TXamlTextBlockType> HRESULT SetWrapProperties(_In_ TXamlTextBlockType* xamlTextBlock, bool wrap)
{
    RETURN_IF_FAILED(xamlTextBlock->put_TextWrapping(wrap ? TextWrapping::TextWrapping_WrapWholeWords : TextWrapping::TextWrapping_NoWrap));
    RETURN_IF_FAILED(xamlTextBlock->put_TextTrimming(TextTrimming::TextTrimming_CharacterEllipsis));
    return S_OK;
}

template<typename TAdaptiveType, typename TXamlTextBlockType>
HRESULT StyleXamlTextBlockProperties(TAdaptiveType* adaptiveElement, _In_ TXamlTextBlockType* xamlTextBlock)
{
    boolean wrap;
    RETURN_IF_FAILED(adaptiveElement->get_Wrap(&wrap));
    RETURN_IF_FAILED(SetWrapProperties(xamlTextBlock, wrap));

    UINT32 maxLines;
    RETURN_IF_FAILED(adaptiveElement->get_MaxLines(&maxLines));
    if (maxLines != MAXUINT32)
    {
        RETURN_IF_FAILED(SetMaxLines(xamlTextBlock, maxLines));
    }

    HAlignment horizontalAlignment;
    RETURN_IF_FAILED(adaptiveElement->get_HorizontalAlignment(&horizontalAlignment));

    switch (horizontalAlignment)
    {
    case ABI::AdaptiveNamespace::HAlignment::Left:
        RETURN_IF_FAILED(xamlTextBlock->put_TextAlignment(TextAlignment::TextAlignment_Left));
        break;
    case ABI::AdaptiveNamespace::HAlignment::Right:
        RETURN_IF_FAILED(xamlTextBlock->put_TextAlignment(TextAlignment::TextAlignment_Right));
        break;
    case ABI::AdaptiveNamespace::HAlignment::Center:
        RETURN_IF_FAILED(xamlTextBlock->put_TextAlignment(TextAlignment::TextAlignment_Center));
        break;
    }

    return S_OK;
}
