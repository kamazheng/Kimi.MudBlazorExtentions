using Kimi.MudBlazorExtentions.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics
{
    public partial class DynamicInput
    {

        [Parameter, EditorRequired]
        public object ClassInstance { get; set; } = null!;
        [Parameter]
        public EventCallback<object> ClassInstanceChanged { get; set; }

        [Parameter, EditorRequired]
        public PropertyInfo PropertyInfo { get; set; } = null!;

        [Parameter]
        public bool? OverrideHelperTextOnFocus { get; set; }

        private PropertyInfo[]? subPropertyInfos = Array.Empty<PropertyInfo>();
        private Type underlyingType = null!;
        bool isNullable = false;
        private object? classPropertyValue;
        private string displayLabel => PropertyInfo.GetDisplayLabel();
        private string? title => PropertyInfo.GetXmlSummary();
        private System.TypeCode typeCode => Type.GetTypeCode(underlyingType);


        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (ClassInstance == null)
            {
                var hostType = PropertyInfo.DeclaringType;
                if (hostType != null)
                {
                    ClassInstance = Activator.CreateInstance(hostType)!;
                    ClassInstanceChanged.InvokeAsync(ClassInstance);
                }
            }

            underlyingType = Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType;
            isNullable = Nullable.GetUnderlyingType(PropertyInfo.PropertyType) != null;
            subPropertyInfos = underlyingType.IsClass && underlyingType != typeof(string) ? underlyingType.GetProperties() : null;
            if (subPropertyInfos != null && subPropertyInfos.Length > 0)
            {
                classPropertyValue = PropertyInfo.GetValue(ClassInstance);
                if (classPropertyValue == null)
                {
                    classPropertyValue = Activator.CreateInstance(PropertyInfo.PropertyType);
                    PropertyInfo.SetValue(ClassInstance, classPropertyValue);
                    ClassInstanceChanged.InvokeAsync(ClassInstance);
                }
            }
            else
            {
                Value = PropertyInfo.GetValue(ClassInstance);
            }
            this.HelperText ??= title;
            this.Label ??= displayLabel;
            HelperTextOnFocus = OverrideHelperTextOnFocus ?? true;
        }
    }
}