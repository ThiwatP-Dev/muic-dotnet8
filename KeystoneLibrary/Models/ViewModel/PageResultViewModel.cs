namespace KeystoneLibrary.Models.ViewModel
{
    public class PageResultViewModel<T> where T : class
    {
        public int TotalItem { get; set; }

        public int TotalPage { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}

