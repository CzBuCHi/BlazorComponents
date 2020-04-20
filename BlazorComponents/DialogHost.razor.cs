using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorComponents
{
    public sealed class DialogsService
    {
        public event Action Update;
        public event Action<(string identifier, string result)> ActionExecuted;

        public List<DialogContext> Dialogs { get; } = new List<DialogContext>();

        public void ShowDialog<T>(DialogInfo info) where T : ComponentBase {
            ShowDialog(typeof(T), info);
        }

        public void ShowDialog(Type componentType, DialogInfo info) {
            if (!typeof(ComponentBase).IsAssignableFrom(componentType)) {
                throw new ArgumentException("Invalid component type", nameof(componentType));
            }

            void Content(RenderTreeBuilder builder) {
                builder.OpenComponent(0, componentType);
                builder.CloseComponent();
            }

            lock (Dialogs) {
                Dialogs.Add(new DialogContext(this, info, Content));
                OnUpdate();
            }
        }

        private void OnUpdate() {
            Update?.Invoke();
        }

        internal void OnActionExecuted((string identifier, string result) item) {
            lock (Dialogs) {
                int index = Dialogs.FindIndex(o => o.Identifier == item.identifier);
                if (index != -1) {
                    Dialogs.RemoveAt(index);
                }
            }

            ActionExecuted?.Invoke(item);
        }
    }

    public class DialogInfo
    {
        public string Identifier { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Header { get; set; }
        public (string title, string result)[] Actions { get; set; }
        public string CancelResult { get; set; }
        public bool Modal { get; set; }
        public bool Percents { get; set; }
        public bool CloseButtonInactive { get; set; }
        public string Icon { get; set; }
    }

    public class DialogContext
    {
        private readonly DialogsService _Service;

        private DialogContext(DialogInfo info) {
            Identifier = info.Identifier;
            Top = info.Top;
            Left = info.Left;
            Width = info.Width;
            Height = info.Height;
            Header = info.Header;
            Actions = info.Actions;
            CancelResult = info.CancelResult;
            Modal = info.Modal;
            Percents = info.Percents;
            CloseButtonInactive = info.CloseButtonInactive;
            Icon = info.Icon;
        }

        public DialogContext(DialogsService service, DialogInfo info, RenderFragment childContent) : this(info) {
            _Service = service;
            ChildContent = childContent;
            OnActionExecuted = _Service.OnActionExecuted;
        }

        public RenderFragment ChildContent { get; }
        public Action<(string identifier, string result)> OnActionExecuted { get; }

        public int ZIndex => 5000 + _Service.Dialogs.IndexOf(this);

        public string Identifier { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Header { get; set; }
        public (string title, string result)[] Actions { get; set; }
        public string CancelResult { get; set; }
        public bool Active => _Service.Dialogs.IndexOf(this) == 0;
        public bool Modal { get; set; }
        public bool Percents { get; set; }
        public bool CloseButtonInactive { get; set; }
        public string Icon { get; set; }
    }

    public abstract class DialogHostBase : ComponentBase, IDisposable
    {
        [Inject]
        public DialogsService DialogsService { get; set; }

        protected override void OnInitialized() {
            base.OnInitialized();
            DialogsService.Update += DialogsServiceOnUpdate;
        }

        private void DialogsServiceOnUpdate() {
            StateHasChanged();
        }

        public void Dispose() {
            DialogsService.Update -= DialogsServiceOnUpdate;
        }
    }
}