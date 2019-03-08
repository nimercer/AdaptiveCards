#pragma once

#include "AdaptiveCards.Rendering.Uwp.h"
#include "Enums.h"
#include "TextRun.h"

namespace AdaptiveNamespace
{
    class DECLSPEC_UUID("d37e5b66-2a5e-4a9e-b087-dbef5a1705b1") AdaptiveTextRun
        : public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::RuntimeClassType::WinRtClassicComMix>,
                                              ABI::AdaptiveNamespace::IAdaptiveTextElement,
                                              ABI::AdaptiveNamespace::IAdaptiveInline,
                                              Microsoft::WRL::CloakedIid<ITypePeek>>
    {
        AdaptiveRuntime(AdaptiveTextRun);

    public:
        HRESULT RuntimeClassInitialize() noexcept;
        HRESULT RuntimeClassInitialize(const std::shared_ptr<AdaptiveSharedNamespace::TextRun>& sharedTextRun);

        // IAdaptiveTextRun
        IFACEMETHODIMP get_Text(_Outptr_ HSTRING* text) { return E_NOTIMPL; }
        IFACEMETHODIMP put_Text(_In_ HSTRING text) { return E_NOTIMPL; }

        IFACEMETHODIMP get_Size(_Out_ ABI::AdaptiveNamespace::TextSize* textSize) { return E_NOTIMPL; }
        IFACEMETHODIMP put_Size(ABI::AdaptiveNamespace::TextSize textSize) { return E_NOTIMPL; }

        IFACEMETHODIMP get_Weight(_Out_ ABI::AdaptiveNamespace::TextWeight* textWeight) { return E_NOTIMPL; }
        IFACEMETHODIMP put_Weight(ABI::AdaptiveNamespace::TextWeight textWeight) { return E_NOTIMPL; }

        IFACEMETHODIMP get_Color(_Out_ ABI::AdaptiveNamespace::ForegroundColor* textColor) { return E_NOTIMPL; }
        IFACEMETHODIMP put_Color(ABI::AdaptiveNamespace::ForegroundColor textColor) { return E_NOTIMPL; }

        IFACEMETHODIMP get_IsSubtle(_Out_ boolean* isSubtle) { return E_NOTIMPL; }
        IFACEMETHODIMP put_IsSubtle(boolean isSubtle) { return E_NOTIMPL; }

        IFACEMETHODIMP get_Language(_Outptr_ HSTRING* language) { return E_NOTIMPL; }
        IFACEMETHODIMP put_Language(_In_ HSTRING language) { return E_NOTIMPL; }

        IFACEMETHODIMP get_FontStyle(_Out_ ABI::AdaptiveNamespace::FontStyle* style) { return E_NOTIMPL; }
        IFACEMETHODIMP put_FontStyle(ABI::AdaptiveNamespace::FontStyle style) { return E_NOTIMPL; }

        HRESULT GetSharedModel(std::shared_ptr<AdaptiveSharedNamespace::TextRun>& sharedModel);

        // ITypePeek method
        void* PeekAt(REFIID riid) override { return PeekHelper(riid, this); }
    };

    ActivatableClass(AdaptiveTextRun);
}
