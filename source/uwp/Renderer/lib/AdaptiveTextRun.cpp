#include "pch.h"
#include "AdaptiveTextRun.h"
#include "Util.h"
#include <windows.foundation.collections.h>

using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace ABI::AdaptiveNamespace;
using namespace ABI::Windows::Foundation::Collections;
using namespace ABI::Windows::UI::Xaml;
using namespace ABI::Windows::UI::Xaml::Controls;

namespace AdaptiveNamespace
{
    HRESULT AdaptiveTextRun::RuntimeClassInitialize() noexcept try
    {
        RuntimeClassInitialize(std::make_shared<TextRun>());
        return S_OK;
    }
    CATCH_RETURN;

    HRESULT AdaptiveTextRun::RuntimeClassInitialize(const std::shared_ptr<AdaptiveSharedNamespace::TextRun>& sharedTextRun)
    {
        return AdaptiveTextElement::InitializeTextElement(sharedTextRun);
    }

    HRESULT AdaptiveTextRun::GetSharedModel(std::shared_ptr<AdaptiveSharedNamespace::TextRun>& sharedModel) try
    {
        std::shared_ptr<AdaptiveSharedNamespace::TextRun> textRun = std::make_shared<AdaptiveSharedNamespace::TextRun>();
        RETURN_IF_FAILED(AdaptiveTextElement::SetTextElementProperties(textRun));

        sharedModel = textRun;
        return S_OK;
    }
    CATCH_RETURN;
}
