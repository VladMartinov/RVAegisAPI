using Google.Protobuf.Collections;
using Google.Protobuf;
using Grpc.Net.Client;

namespace RVAegisGrpcService
{
    public class PythonFaceRecognitionClient
    {
        private readonly FaceRecognition.FaceRecognitionClient _client;

        public PythonFaceRecognitionClient(string pythonGrpcServerAddress)
        {
            var channel = GrpcChannel.ForAddress(pythonGrpcServerAddress);
            _client = new FaceRecognition.FaceRecognitionClient(channel);
        }

        public async Task<bool> SendImagesToPythonAsync(List<byte[]> images, List<string> labels)
        {
            var imageByteStrings = new RepeatedField<ByteString>();
            foreach (var image in images)
            {
                imageByteStrings.Add(ByteString.CopyFrom(image));
            }

            var request = new ImageRequest { Images = { imageByteStrings }, Labels = { labels } };
            var response = await _client.SendImagesAsync(request);
            return response.Success;
        }

        public async Task<ResultResponse> GetResultsFromPythonAsync()
        {
            var request = new ResultRequest();
            var response = await _client.GetResultsAsync(request);
            return response;
        }
    }
}
