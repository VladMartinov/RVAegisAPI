using RVAegisGrpcService;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� gRPC
builder.Services.AddGrpc();
builder.Services.AddHttpClient();

var app = builder.Build();

// ����������� gRPC ������
app.MapGrpcService<FaceRecognitionService>();

// ������� HTTP �������� ����� ��� ��������
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();