namespace KeystoneLibrary.Exceptions
{
    public static class InvoiceException
    {
        private static readonly string KEY = "API";
        public static ResultException OrderNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Order Not Found"
                   };
        }

        public static ResultException UnableSaveUpdateIsPaid()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Forbidden, KEY) }002",
                       Message = "Unable to update status paid, invalid input in some fields"
                   };
        }
    }
}