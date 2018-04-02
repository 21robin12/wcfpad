using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WcfPad.UI.Controllers;

namespace WcfPad.UI
{
    public class AjaxObject
    {
        public void Execute(string guid, string controllerName, string methodName, IList<object> data = null)
        {
            var controllerType = GetControllerType(controllerName);
            var controller = GetController(controllerType);

            var method = controllerType.GetMethod(methodName);
            Task.Factory
                .StartNew(() => method.Invoke(controller, data?.ToArray()))
                .ContinueWith((e) =>
                {
                    CefForm.Instance.ExecuteJsFunction("ajax.resolve", guid, e.Result?.ToString());
                });
        }

        private Type GetControllerType(string controllerName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            var controllerType = types.FirstOrDefault(x => typeof(Controller).IsAssignableFrom(x) && x.Name == controllerName);
            if (controllerType == null)
            {
                throw new Exception($"Class extending Controller with name {controllerName} was not found in {assembly.FullName}");
            }

            return controllerType;
        }

        private object GetController(Type controllerType)
        {
            var methodInfo = typeof(DependencyInjection).GetMethod(nameof(DependencyInjection.Get));
            var generic = methodInfo.MakeGenericMethod(controllerType);
            var controller = generic.Invoke(null, null);
            return controller;
        }
    }
}
