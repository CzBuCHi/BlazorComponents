using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorComponents
{
    [EventHandler("onmouseenter", typeof(MouseEventArgs), true, true)]
    [EventHandler("onmouseleave", typeof(MouseEventArgs), true, true)]
    public static class EventHandlers
    {
    }
}
