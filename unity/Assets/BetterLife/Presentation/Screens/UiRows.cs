using System;
using BetterLife.Core.ViewModels;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Builds the repeated pieces of list screens (section titles, label/value rows, buttons, result cards) with
    /// consistent USS classes.
    /// </summary>
    public static class UiRows
    {
        /// <summary>Class of a label/value row.</summary>
        public const string RowClass = "value-row";
        /// <summary>Class of a good row.</summary>
        public const string GoodClass = "value-row--good";
        /// <summary>Class of a bad row.</summary>
        public const string BadClass = "value-row--bad";
        /// <summary>Class of a tappable row.</summary>
        public const string TappableClass = "value-row--tappable";

        /// <summary>
        /// Creates a label with a class.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="ussClass">USS class.</param>
        /// <returns>The label.</returns>
        public static Label Text(string text, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            return label;
        }

        /// <summary>
        /// Creates a section title.
        /// </summary>
        /// <param name="text">Title.</param>
        /// <returns>The label.</returns>
        public static Label Section(string text) => Text(text, "section-title");

        /// <summary>
        /// Creates a label/value row (with an optional detail line).
        /// </summary>
        /// <param name="row">Row.</param>
        /// <param name="onTap">Called when tapped; null for a plain row.</param>
        /// <returns>The element.</returns>
        public static VisualElement Row(ValueRow row, Action onTap = null)
        {
            return Row(row.Label, row.Value, row.Detail, row.Tone, onTap);
        }

        /// <summary>
        /// Creates a label/value row.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="value">Value text.</param>
        /// <param name="detail">Detail line, or empty.</param>
        /// <param name="tone">Colour of the value.</param>
        /// <param name="onTap">Called when tapped; null for a plain row.</param>
        /// <returns>The element.</returns>
        public static VisualElement Row(string label, string value, string detail, RowTone tone, Action onTap = null)
        {
            var element = new VisualElement();
            element.AddToClassList(RowClass);
            element.EnableInClassList(GoodClass, tone == RowTone.Good);
            element.EnableInClassList(BadClass, tone == RowTone.Bad);
            var text = new VisualElement();
            text.AddToClassList("value-row__text");
            text.Add(Text(label, "value-row__label"));
            if (!string.IsNullOrEmpty(detail))
            {
                text.Add(Text(detail, "value-row__detail"));
            }
            element.Add(text);
            element.Add(Text(value ?? string.Empty, "value-row__value"));
            if (onTap != null)
            {
                element.AddToClassList(TappableClass);
                element.AddManipulator(new Clickable(onTap));
            }
            return element;
        }

        /// <summary>
        /// Creates an action button.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="onClick">Called when pressed.</param>
        /// <param name="enabled">Whether it can be pressed.</param>
        /// <param name="detail">Line under the button (chance, cost, reason), or empty.</param>
        /// <returns>The element (button plus detail).</returns>
        public static VisualElement Action(string text, Action onClick, bool enabled = true, string detail = "")
        {
            var block = new VisualElement();
            block.AddToClassList("action");
            var button = new Button(onClick) { text = text };
            button.AddToClassList("action__button");
            button.SetEnabled(enabled);
            block.Add(button);
            if (!string.IsNullOrEmpty(detail))
            {
                block.Add(Text(detail, "action__detail"));
            }
            return block;
        }

        /// <summary>
        /// Creates a result card shown after an action.
        /// </summary>
        /// <param name="result">Result.</param>
        /// <param name="closeText">Close button text.</param>
        /// <param name="onClose">Called when closed.</param>
        /// <returns>The element.</returns>
        public static VisualElement Result(ActionResultState result, string closeText, Action onClose)
        {
            var card = new VisualElement();
            card.AddToClassList("result-card");
            card.EnableInClassList("result-card--failed", !result.Succeeded);
            card.Add(Text(result.Title, "result-card__title"));
            foreach (string line in result.Lines)
            {
                card.Add(Text(line, "result-card__line"));
            }
            var close = new Button(onClose) { text = closeText };
            close.AddToClassList("result-card__close");
            card.Add(close);
            return card;
        }
    }
}
