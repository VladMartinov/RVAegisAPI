using Google.Protobuf;
using Grpc.Net.Client;
using Google.Protobuf.Collections;
using RVAegis;

public class FaceRecognitionClient
{
    private readonly FaceRecognition.FaceRecognitionClient _client;

    public FaceRecognitionClient(string grpcServerAddress)
    {
        var channel = GrpcChannel.ForAddress(grpcServerAddress);
        _client = new FaceRecognition.FaceRecognitionClient(channel);
    }

    public async Task<bool> SendImagesAsync(List<byte[]> images, List<string> labels)
    {
        // Преобразуем List<byte[]> в RepeatedField<ByteString>
        var imageByteStrings = new RepeatedField<ByteString>();
        foreach (var image in images)
        {
            imageByteStrings.Add(ByteString.CopyFrom(image));
        }

        var request = new ImageRequest { Images = { imageByteStrings }, Labels = { labels } };
        var response = await _client.SendImagesAsync(request);
        return response.Success;
    }

    public async Task<ResultResponse> GetResultsAsync()
    {
        var request = new ResultRequest();
        var response = await _client.GetResultsAsync(request);
        return response;
    }
}