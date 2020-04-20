using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorComponents
{
    public class DialogContext
    {
        private readonly DialogsService _Service;

        public DialogContext(DialogsService service, DialogInfo info, RenderFragment childContent) : this(info) {
            _Service = service;
            ChildContent = childContent;
            OnActionExecuted = service.OnActionExecuted;
        }

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

        public bool Active {
            get { return _Service.Dialogs.IndexOf(this) == 0; }
        }

        public RenderFragment ChildContent { get; }
        public Action<string, string> OnActionExecuted { get; }

        public int ZIndex {
            get { return 5000 + _Service.Dialogs.IndexOf(this); }
        }

        public (string title, string result)[] Actions { get; set; }
        public string CancelResult { get; set; }
        public bool CloseButtonInactive { get; set; }
        public string Header { get; set; }
        public double Height { get; set; }
        public string Icon { get; set; }

        public string Identifier { get; set; }
        public double Left { get; set; }
        public bool Modal { get; set; }
        public bool Percents { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
    }

    public abstract class DialogHostBase : ComponentBase, IDisposable
    {
        [Inject]
        public DialogsService DialogsService { get; set; }

        public void Dispose() {
            DialogsService.Update -= DialogsServiceOnUpdate;
        }

        protected override void OnInitialized() {
            base.OnInitialized();
            DialogsService.Update += DialogsServiceOnUpdate;
        }

        private void DialogsServiceOnUpdate() {
            StateHasChanged();
        }
    }

    public class DialogInfo
    {
        public (string title, string result)[] Actions { get; set; }
        public string CancelResult { get; set; }
        public bool CloseButtonInactive { get; set; }
        public string Header { get; set; }
        public int Height { get; set; }
        public string Icon { get; set; }
        public string Identifier { get; set; }
        public int Left { get; set; }
        public bool Modal { get; set; }
        public bool Percents { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
    }

    public sealed class DialogsService
    {
        public event Action<string, string> ActionExecuted;
        public event Action Update;

        public List<DialogContext> Dialogs { get; } = new List<DialogContext>();

        public DialogContext FindDialogContext(string identifier) {
            return Dialogs.Find(o => o.Identifier == identifier);
        }

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

        internal void OnActionExecuted(string identifier, string result) {
            lock (Dialogs) {
                int index = Dialogs.FindIndex(o => o.Identifier == identifier);
                if (index != -1) {
                    Dialogs.RemoveAt(index);
                    OnUpdate();
                }
            }

            ActionExecuted?.Invoke(identifier, result);
        }

        private void OnUpdate() {
            Update?.Invoke();
        }
    }
}
