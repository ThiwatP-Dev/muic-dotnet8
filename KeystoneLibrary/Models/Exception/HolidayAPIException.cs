namespace KeystoneLibrary.Exceptions
{
    public static class HolidayAPIException
    {
        private static readonly string KEY = "API_HOLIDAY";

        public static ResultException DateTimeWrongFormat(string datetime)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}001",
                Message = $"Given Date ({datetime}) is invalid. Format = (dd/mm/yyyy HH:MM)"
            };
        }

        public static ResultException WrongDateRange(string startedAt, string endedAt)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}002",
                Message = $"Given Date startedAt ({startedAt}) must come before endedAt ({endedAt})"
            };
        }

        public static ResultException WrongSectionIds(List<long> sectionIds)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}003",
                Message = $"Given 'CancelSectionIds' contain id that are not exists [{string.Join(",", sectionIds)}]"
            };
        }

        public static ResultException UpdateError(string msg)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}004",
                Message = $"DB operation error: {msg}"
            };
        }
    }
}