using System;
using System.Dynamic;
using System.Reflection;

namespace Kapok.Report.Razor
{
    internal class AnonymousTypeWrapper : DynamicObject
    {
        private readonly object _model;
        private readonly Type _modelType;

        public AnonymousTypeWrapper(object model)
        {
            _model = model;
            _modelType = model.GetType();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            PropertyInfo? propertyInfo = _modelType.GetProperty(binder.Name);
            if (propertyInfo == null)
            {
                result = null;
                return false;
            }

            result = propertyInfo.GetValue(_model, null);

            // nested objects and array handling goes here
            // full code: https://github.com/adoconnection/RazorEngineCore/blob/master/
            // RazorEngineCore/AnonymousTypeWrapper.cs

            return true;
        }
    }
}