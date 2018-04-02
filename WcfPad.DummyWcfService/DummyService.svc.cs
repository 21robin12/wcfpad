using System;
using System.Linq;

namespace WcfPad.DummyWcfService
{
    public class DummyService : IDummyService
    {
        public Response GetRequestObject(Request request)
        {
            var response = new Response
            {
                Item = MapItem(request.Item),
                ItemsEnumerable = request.ItemsEnumerable == null ? null : request.ItemsEnumerable.Select(MapItem),
                ItemsList = request.ItemsList == null ? null : request.ItemsList.Select(MapItem).ToList(),
                ItemsArray = request.ItemsArray == null ? null : request.ItemsArray.Select(MapItem).ToArray()
            };

            MapProperties(request, response);
            return response;
        }

        public string GetString(int value)
        {
            return $"You sent {value}";
        }

        private void MapProperties(IWcfProperties from, IWcfProperties to)
        {
            to.Enum = from.Enum;
            to.Int = from.Int;
            to.String = from.String;
            to.Double = from.Double;
            to.Bool = from.Bool;
            to.DateTime = from.DateTime;
            to.Dictionary = from.Dictionary;
            to.KeyValuePair = from.KeyValuePair;
            to.Guid = from.Guid;
            to.TimeSpan = from.TimeSpan;
        }

        private ResponseItem MapItem(RequestItem requestItem)
        {
            if (requestItem == null)
            {
                return null;
            }

            var responseItem = new ResponseItem();
            MapProperties(requestItem, responseItem);
            return responseItem;
        }

        public int GetNumberFromParams(params int[] inputs)
        {
            return inputs.Sum();
        }

        public bool GetBoolNoParameters()
        {
            return true;
        }

        public string GetStringOutRefParameters(out int i, ref bool b, string anotherParameter)
        {
            i = 1337;
            b = !b;

            return $"Success! You sent b={b} and anotherParameter=\"{anotherParameter}\". i was set to {i} and b was inverted.";
        }

        public CircularResponse GetCircularResponse(CircularRequest request)
        {
            var response = new CircularResponse
            {
                Item = From(request.Item)
            };

            return response;
        }
         
        public CircularResponse GetSelfReferentialResponse()
        {
            var item = new CircularResponseItem
            {
                Value = 99
            };

            item.Child = item;

            var response = new CircularResponse
            {
                Item = item
            };

            return response;
        }

        private CircularResponseItem From(CircularRequestItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new CircularResponseItem
            {
                Value = item.Value,
                Child = From(item.Child)
            };
        }

        private CircularResponseItem From(CircularRequestItem2 item)
        {
            if (item == null)
            {
                return null;
            }

            return new CircularResponseItem
            {
                Value = item.Value,
                Child = From(item.Child)
            };
        }
    }
}
