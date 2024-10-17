namespace KeystoneLibrary.Exceptions
{
    public static class RoomReservationAPIException
    {
        private static readonly string KEY = "API";
        
        public static ResultException AvailableRoomNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Available Room Not Found"
                   };
        }

        public static ResultException StudentNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }002",
                       Message = "Student Not Found"
                   };
        }

        public static ResultException UnableToReserve()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }003",
                       Message = "Unable to reserve"
                   };
        }

        public static ResultException OutOfReservationPeriod()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Out of reservation period"
                   };
        }

        public static ResultException RoomNotAvailable()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }005",
                       Message = "Room is not available"
                   };
        }

        public static ResultException BuildingNotAvailable()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }006",
                       Message = "Building is not available"
                   };
        }

        public static ResultException ReservationNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }007",
                       Message = "Reservation not found"
                   };
        }

        public static ResultException UnableToDelete()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }008",
                       Message = "Unable to delete"
                   };
        }

        public static ResultException InvalidUsingType()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }009",
                       Message = "Invalid using type"
                   };
        }
    }
}