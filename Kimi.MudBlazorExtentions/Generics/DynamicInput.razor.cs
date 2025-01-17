using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Kimi.MudBlazorExtentions.Generics
{
    public partial class DynamicInput
    {
        [Parameter]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        [Parameter, EditorRequired]
        public object ClassInstance { get; set; } = null!;
        [Parameter]
        public EventCallback<object> ClassInstanceChanged { get; set; }

        [Parameter, EditorRequired]
        public PropertyInfo PropertyInfo { get; set; } = null!;

        private PropertyInfo[]? subPropertyInfos = Array.Empty<PropertyInfo>();
        private Type? underlyingType;
        private object? classPropertyValue;

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
        }
    }
}