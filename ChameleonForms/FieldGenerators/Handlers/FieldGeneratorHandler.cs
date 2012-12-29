﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ChameleonForms.Component.Config;
using ChameleonForms.Enums;
using ChameleonForms.Templates;
using System.Linq;

namespace ChameleonForms.FieldGenerators.Handlers
{
    internal abstract class FieldGeneratorHandler<TModel, T>
    {
        protected readonly IFieldGenerator<TModel, T> FieldGenerator;
        protected readonly IFieldConfiguration FieldConfiguration;

        protected FieldGeneratorHandler(IFieldGenerator<TModel, T> fieldGenerator, IFieldConfiguration fieldConfiguration)
        {
            FieldGenerator = fieldGenerator;
            FieldConfiguration = fieldConfiguration;
        }

        public abstract HandleAction Handle();

        protected bool HasMultipleValues()
        {
            return FieldGenerator.Metadata.ModelType.IsGenericType
                && typeof(IEnumerable).IsAssignableFrom(FieldGenerator.Metadata.ModelType);
        }

        protected IEnumerable<object> GetValues()
        {
            return (((IEnumerable)FieldGenerator.GetValue()) ?? new object[]{}).Cast<object>();
        }

        protected bool IsSelected(object value)
        {
            return HasMultipleValues()
                ? GetValues().Contains(value)
                : FieldGenerator.GetValue().Equals(value);
        }

        public Type GetUnderlyingType()
        {
            var type = FieldGenerator.Metadata.ModelType;

            if (HasMultipleValues())
                type = type.GetGenericArguments()[0];

            return Nullable.GetUnderlyingType(type) ?? type;
        }

        protected IHtmlString GetInputHtml(TextInputType inputType)
        {
            if (inputType == TextInputType.Password)
                return FieldGenerator.HtmlHelper.PasswordFor(FieldGenerator.FieldProperty, FieldConfiguration.Attributes.ToDictionary());

            FieldConfiguration.Attributes.Attr(type => inputType.ToString().ToLower());
            return FieldGenerator.HtmlHelper.TextBoxFor(FieldGenerator.FieldProperty, FieldConfiguration.Attributes.ToDictionary());
        }

        protected IHtmlString GetSelectListHtml(IEnumerable<SelectListItem> selectList)
        {
            if (!FieldGenerator.Metadata.IsRequired && !(FieldConfiguration.DisplayType == FieldDisplayType.List && HasMultipleValues()))
                selectList = new []{GetEmptySelectListItem()}.Union(selectList);

            switch (FieldConfiguration.DisplayType)
            {
                case FieldDisplayType.List:
                    var list = SelectListToRadioList(selectList);
                    return HtmlHelpers.List(list);
                case FieldDisplayType.DropDown:
                case FieldDisplayType.Default:
                    if (HasMultipleValues())
                        FieldConfiguration.Attr("multiple", "multiple");
                    return FieldGenerator.HtmlHelper.DropDownListFor(
                        FieldGenerator.FieldProperty, selectList,
                        FieldConfiguration.Attributes.ToDictionary()
                    );
            }

            return null;
        }

        private SelectListItem GetEmptySelectListItem()
        {
            var selected = FieldGenerator.GetValue() == null;
            if (typeof (T) == typeof (string))
                selected = string.IsNullOrEmpty(FieldGenerator.GetValue() as string);
            return new SelectListItem
            {
                Selected = selected,
                Value = "",
                Text = string.IsNullOrEmpty(FieldConfiguration.NoneString)
                        &&
                        (FieldConfiguration.DisplayType == FieldDisplayType.List && !HasMultipleValues())
                        ||
                        (FieldConfiguration.DisplayType != FieldDisplayType.List && HasMultipleValues())
                    ? "None"
                    : FieldConfiguration.NoneString
            };
        }

        private IEnumerable<IHtmlString> SelectListToRadioList(IEnumerable<SelectListItem> selectList)
        {
            var count = 0;
            foreach (var item in selectList)
            {
                var id = string.Format("{0}_{1}", GetFieldName(), ++count);
                var attrs = new HtmlAttributes(FieldConfiguration.Attributes.ToDictionary());
                if (item.Selected)
                    attrs.Attr("checked", "checked");
                attrs.Attr("id", id);
                if (HasMultipleValues())
                    AdjustHtmlForModelState(attrs);
                yield return new HtmlString(string.Format("{0} {1}",
                    HasMultipleValues()
                        ? HtmlCreator.BuildSingleCheckbox(GetFieldName(), item.Selected, attrs, item.Value)
                        : FieldGenerator.HtmlHelper.RadioButtonFor(FieldGenerator.FieldProperty, item.Value, attrs.ToDictionary()),
                    FieldGenerator.HtmlHelper.Label(id, item.Text)
                ));
            }
        }
        
        protected string GetFieldName()
        {
            var name = ExpressionHelper.GetExpressionText(FieldGenerator.FieldProperty);
            return FieldGenerator.HtmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
        }
        
        protected void AdjustHtmlForModelState(HtmlAttributes attrs)
        {
            var name = ExpressionHelper.GetExpressionText(FieldGenerator.FieldProperty);
            var fullName = FieldGenerator.HtmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            ModelState modelState;
            if (FieldGenerator.HtmlHelper.ViewContext.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    attrs.AddClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            attrs.Attrs(FieldGenerator.HtmlHelper.GetUnobtrusiveValidationAttributes(name, ModelMetadata.FromLambdaExpression(FieldGenerator.FieldProperty, FieldGenerator.HtmlHelper.ViewData)));
        }
    }

}