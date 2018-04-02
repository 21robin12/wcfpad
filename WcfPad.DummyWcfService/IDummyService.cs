using System.ServiceModel;

namespace WcfPad.DummyWcfService
{
    [ServiceContract]
    public interface IDummyService
    {
        [OperationContract]
        string GetString(int value);

        [OperationContract]
        Response GetRequestObject(Request request);

        [OperationContract]
        int GetNumberFromParams(params int[] inputs);

        [OperationContract]
        bool GetBoolNoParameters();

        [OperationContract]
        string GetStringOutRefParameters(out int i, ref bool b, string anotherParameter);

        [OperationContract]
        CircularResponse GetCircularResponse(CircularRequest request);

        [OperationContract]
        CircularResponse GetSelfReferentialResponse();
    }
}
