#include "pch.h"
#include "AdaptiveParagraph.h"
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
    HRESULT AdaptiveParagraph::RuntimeClassInitialize() noexcept try
    {
        RuntimeClassInitialize(std::make_shared<Paragraph>());
        return S_OK;
    }
    CATCH_RETURN;

    HRESULT AdaptiveParagraph::RuntimeClassInitialize(const std::shared_ptr<AdaptiveSharedNamespace::Paragraph>& sharedParagraph)
    {
        return S_OK;
    }

    HRESULT AdaptiveParagraph::get_Inlines(ABI::Windows::Foundation::Collections::IVector<ABI::AdaptiveNamespace::IAdaptiveInline*>** inlines)
    {
        return E_NOTIMPL;
    }

    HRESULT AdaptiveParagraph::GetSharedModel(std::shared_ptr<AdaptiveSharedNamespace::Paragraph>& sharedModel) try
    {
        std::shared_ptr<AdaptiveSharedNamespace::Paragraph> paragraph = std::make_shared<AdaptiveSharedNamespace::Paragraph>();
        sharedModel = paragraph;
        return S_OK;
    }
    CATCH_RETURN;
}
