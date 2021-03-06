﻿using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace ChameleonForms.Component
{
    /// <summary>
    /// Chameleon Forms component - holds a reference to a form.
    /// </summary>
    /// <typeparam name="TModel">The view model type for the current view</typeparam>
    public interface IFormComponent<TModel>
    {
        /// <summary>
        /// The form that the component is attached to.
        /// </summary>
        IForm<TModel> Form { get; }
    }

    /// <summary>
    /// Chameleon Forms base component class; provides an ability to easily write HTML to the page in a self-closing or nested manner.
    /// Ensure you call Initialise() at the end of the constructor when extending this class.
    /// </summary>
    public abstract class FormComponent<TModel> : IFormComponent<TModel>, IHtmlContent, IDisposable
    {
        /// <summary>
        /// Whether or not the component is self-closing when instantiated or Dispose will be called later.
        /// </summary>
        protected readonly bool IsSelfClosing;

        /// <inheritdoc />
        public IForm<TModel> Form { get; private set; }

        /// <summary>
        /// Create a form component.
        /// </summary>
        /// <param name="form">The form</param>
        /// <param name="isSelfClosing">Whether or not the component is self closing or has an explicit end tag</param>
        protected FormComponent(IForm<TModel> form, bool isSelfClosing)
        {
            Form = form;
            IsSelfClosing = isSelfClosing;
        }

        /// <summary>
        /// Initialises the form component; should be called at the end of the constructor of any derived classes.
        /// Writes HTML directly to the page is the component isn't self-closing
        /// </summary>
        public void Initialise()
        {
            if (!IsSelfClosing)
                Form.Write(Begin());
        }

        /// <summary>
        /// Returns the HTML representation of the beginning of the form component.
        /// </summary>
        /// <returns>The beginning HTML for the form component</returns>
        public abstract IHtmlContent Begin();

        /// <summary>
        /// Returns the HTML representation of the end of the form component.
        /// </summary>
        /// <returns>The ending HTML for the form component</returns>
        public abstract IHtmlContent End();

        /// <summary>
        /// Called when form component outputted to the page; writes the form content HTML to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="encoder">The HTML encoder to use when writing</param>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (!IsSelfClosing)
                return;

            Begin().WriteTo(writer, encoder);
            End().WriteTo(writer, encoder);
        }

        /// <summary>
        /// Called when form component is created within a `using` block: writes the end tag(s) of the component.
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsSelfClosing)
                Form.Write(End());
        }
    }
}
