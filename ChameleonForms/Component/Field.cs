﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using ChameleonForms.Component.Config;
using ChameleonForms.FieldGenerators;
using Microsoft.AspNetCore.Html;

namespace ChameleonForms.Component
{
    /// <summary>
    /// Helper for field configuration.
    /// </summary>
    public static class Field
    {
        /// <summary>
        /// Returns a field configuration object.
        /// </summary>
        /// <returns>A field configuration</returns>
        public static FieldConfiguration Configure()
        {
            return new FieldConfiguration();
        }
    }

    /// <summary>
    /// Wraps the output of a single form field.
    /// </summary>
    /// <typeparam name="TModel">The view model type for the current view</typeparam>
    
    public class Field<TModel> : FormComponent<TModel>
    {
        private readonly IFieldGenerator _fieldGenerator;
        private readonly IFieldConfiguration _config;
        private bool IsParent { get { return !IsSelfClosing; } }
        
        /// <summary>
        /// Creates a form field.
        /// </summary>
        /// <param name="form">The form the field is being created in</param>
        /// <param name="isParent">Whether or not the field has other fields nested within it</param>
        /// <param name="fieldGenerator">A field HTML generator class</param>
        /// <param name="config">The configuration values for the field</param>
        public Field(IForm<TModel> form, bool isParent, IFieldGenerator fieldGenerator, IFieldConfiguration config)
            : base(form, !isParent)
        {
            if (isParent)
                form.HtmlHelper.ViewData[Constants.ViewDataFieldKey] = this;

            _fieldGenerator = fieldGenerator;
            _config = config;
            if (_config != null)
                _config.SetField(this);
            Initialise();
        }

        /// <inheritdoc />
        public override IHtmlContent Begin()
        {
            var isValid = Form.HtmlHelper.ViewData.ModelState.GetFieldValidationState(_fieldGenerator.GetFieldId()) != Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            var readonlyConfig = _fieldGenerator.PrepareFieldConfiguration(_config, FieldParent.Section);
            return !IsParent
                ? Form.Template.Field(_fieldGenerator.GetLabelHtml(readonlyConfig), _fieldGenerator.GetFieldHtml(readonlyConfig), _fieldGenerator.GetValidationHtml(readonlyConfig), _fieldGenerator.Metadata, readonlyConfig, isValid)
                : Form.Template.BeginField(_fieldGenerator.GetLabelHtml(readonlyConfig), _fieldGenerator.GetFieldHtml(readonlyConfig), _fieldGenerator.GetValidationHtml(readonlyConfig), _fieldGenerator.Metadata, readonlyConfig, isValid);
        }

        /// <inheritdoc />
        public override IHtmlContent End()
        {
            return !IsParent
                ? new HtmlString(string.Empty)
                : Form.Template.EndField();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            Form.HtmlHelper.ViewData.Remove(Constants.ViewDataFieldKey);
        }
    }

    /// <summary>
    /// Extension methods for the creation of form fields, labels and validation messages.
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>
        /// Creates a single form field as a child of a form section.
        /// </summary>
        /// <example>
        /// @s.FieldFor(m => m.FirstName)
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="section">The section the field is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the field for</param>
        /// <param name="config">Optional base field configuration</param>
        /// <returns>A field configuration object that allows you to configure the field</returns>
        public static IFieldConfiguration FieldFor<TModel, T>(this ISection<TModel> section, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            var fc = config ?? new FieldConfiguration();
            // ReSharper disable ObjectCreationAsStatement
            new Field<TModel>(section.Form, false, section.Form.GetFieldGenerator(property), fc);
            // ReSharper restore ObjectCreationAsStatement
            return fc;
        }

        /// <summary>
        /// Creates a single form field as a child of a form section that can have other form fields nested within it.
        /// </summary>
        /// <example>
        /// @using (var f = s.BeginFieldFor(m => m.Company)) {
        ///     @f.FieldFor(m => m.PositionTitle)
        /// }
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="section">The section the field is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the field for</param>
        /// <param name="config">Any configuration information for the field</param>
        /// <returns>The form field</returns>
        public static Field<TModel> BeginFieldFor<TModel, T>(this ISection<TModel> section, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            return new Field<TModel>(section.Form, true, section.Form.GetFieldGenerator(property), config);
        }

        /// <summary>
        /// Creates a single form field as a child of another form field.
        /// </summary>
        /// <example>
        /// @using (var f = s.BeginFieldFor(m => m.Company)) {
        ///     @f.FieldFor(m => m.PositionTitle)
        /// }
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="field">The parent field the field is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the field for</param>
        /// <param name="config">Optional base field configuration</param>
        /// <returns>A field configuration object that allows you to configure the field</returns>
        public static IFieldConfiguration FieldFor<TModel, T>(this Field<TModel> field, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            var fc = config ?? new FieldConfiguration();
            // ReSharper disable ObjectCreationAsStatement
            new Field<TModel>(field.Form, false, field.Form.GetFieldGenerator(property), fc);
            // ReSharper restore ObjectCreationAsStatement
            return fc;
        }

        /// <summary>
        /// Creates a standalone form field to be output in a form.
        /// </summary>
        /// <example>
        /// @using (var f = Html.BeginChameleonForm()) {
        ///     @f.FieldElementFor(m => m.PositionTitle)
        /// }
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="form">The form the field is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the field for</param>
        /// <param name="config">Optional base field configuration</param>
        /// <returns>A field configuration object that allows you to configure the field</returns>
        public static IFieldConfiguration FieldElementFor<TModel, T>(this IForm<TModel> form, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            var fc = config ?? new FieldConfiguration();
            fc.SetField(() => form.GetFieldGenerator(property).GetFieldHtml(fc));
            return fc;
        }

        /// <summary>
        /// Creates a standalone label to be output in a form for a field.
        /// </summary>
        /// <example>
        /// @using (var f = Html.BeginChameleonForm()) {
        ///     @f.LabelFor(m => m.PositionTitle)
        /// }
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="form">The form the label is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the label for</param>
        /// <param name="config">Optional base field configuration</param>
        /// <returns>The HTML for the label</returns>
        public static IFieldConfiguration LabelFor<TModel, T>(this IForm<TModel> form, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            var fc = config ?? new FieldConfiguration();
            fc.SetField(() => form.GetFieldGenerator(property).GetLabelHtml(fc));
            return fc;
        }

        /// <summary>
        /// Creates a standalone validation message to be output in a form for a field.
        /// </summary>
        /// <example>
        /// @using (var f = Html.BeginChameleonForm()) {
        ///     @f.ValidationMessageFor(m => m.PositionTitle)
        /// }
        /// </example>
        /// <typeparam name="TModel">The view model type for the current view</typeparam>        
        /// <typeparam name="T">The type of the field being generated</typeparam>
        /// <param name="form">The form the label is being created in</param>
        /// <param name="property">A lamdba expression to identify the field to render the validation message for</param>
        /// <param name="config">Optional base field configuration</param>
        /// <returns>The HTML for the validation message</returns>
        public static IFieldConfiguration ValidationMessageFor<TModel, T>(this IForm<TModel> form, Expression<Func<TModel, T>> property, IFieldConfiguration config = null)
        {
            var fc = config ?? new FieldConfiguration();
            fc.SetField(() => form.GetFieldGenerator(property).GetValidationHtml(fc));
            return fc;
        }
    }
}