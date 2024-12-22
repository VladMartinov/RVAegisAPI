namespace RVAegisGrpcService
{
    public class MultipleImageResponseModel
    {
        public string status { get; set; }
        public int count { get; set; }
        
        public MultipleImageResponseModel(string status, int count)
        {
            this.status = status;
            this.count = count;
        }
    }

    public class ImageLoadErrorModel
    {
        public string filename { get; set; }
        public string error { get; set; }

        public ImageLoadErrorModel(string filename, string error)
        {
            this.filename = filename;
            this.error = error;
        }
    }

    public class MultipleImageErrorResponseModel
    {
        public string status { get; set; }
        public List<ImageLoadError> results { get; set; }
    
        public MultipleImageErrorResponseModel(string status, List<ImageLoadError> results)
        {
            this.status = status;
            this.results = results;
        }
    }
}
