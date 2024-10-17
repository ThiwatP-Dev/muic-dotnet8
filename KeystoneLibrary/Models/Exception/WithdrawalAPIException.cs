namespace KeystoneLibrary.Exceptions
{
    public static class WithdrawalAPIException
    {
        private static readonly string KEY = "API";

        public static ResultException StudentsNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Students Not Found"
                   };
        }

        public static ResultException WithdrawalPeriodNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }002",
                       Message = "Withdrawal period not found"
                   };
        }

        public static ResultException NotInWithdrawalPeriod()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }003",
                       Message = "Not in withdrawal period"
                   };
        }

        public static ResultException RegistrationCourseNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Registration course not found"
                   };
        }

        public static ResultException WithdrawalGradeNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }005",
                       Message = "Withdrawal grade not found"
                   };
        }

        public static ResultException WithdrawalNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }006",
                       Message = "Withdrawal not found"
                   };
        }

        public static ResultException ExistWithdrawal()
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}007",
                Message = "Already request Withdrawal"
            };
        }

        public static ResultException MinimumCreditLimit()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }008",
                       Message = "Minimum credit limit"
                   };
        }

        public static ResultException SectionNotAllowWithdrawal()
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}009",
                Message = "Section doesn't allow withdrawal"
            };
        }
    }
}