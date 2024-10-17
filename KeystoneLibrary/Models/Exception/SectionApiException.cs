namespace KeystoneLibrary.Exceptions
{
    public static class SectionApiException
    {
        private static readonly string KEY = "API";
        public static ResultException InstructorNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Instructor Not Found"
                   };
        }

        public static ResultException TermNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }002",
                       Message = "Term Not Found"
                   };
        }

        public static ResultException TimeSlotStatusNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }003",
                       Message = "TimeSlot Status Not Found"
                   };
        }

        public static ResultException ExaminationStatusNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Examination Status Not Found"
                   };
        }

        public static ResultException RoomNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }005",
                       Message = "Room Not Found"
                   };
        }

        public static ResultException SectionNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }006",
                       Message = "Section Not Found"
                   };
        }

        public static ResultException ExaminationNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }007",
                       Message = "Examination Not Found"
                   };
        }

        public static ResultException TimeSlotNotUpdate()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }008",
                       Message = "Unable to Update Time Slot"
                   };
        }

        public static ResultException SectionSlotNotAllow()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }008",
                       Message = "Invalid Date - University Holiday - or reserved date"
                   };
        }


        public static ResultException ExaminationNotUpdate()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }009",
                       Message = "Unable to Update Examination"
                   };
        }

        public static ResultException ExaminationTypeNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }010",
                       Message = "Examination Type Not Found"
                   };
        }

        public static ResultException ExaminationAlreadyApproved()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }011",
                       Message = "Examination is already approved"
                   };
        }

        public static ResultException TeachingTypesNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }012",
                       Message = "Teaching Type Not Found"
                   };
        }
        public static ResultException SectionSlotNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }013",
                       Message = "Section Slot Not Found"
                   };
        }

        public static ResultException ExaminationOverlap()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }014",
                       Message = "conflict with exam timeslots"
                   };
        }
        public static ResultException SectionSlotOverlap()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }015",
                       Message = "conflict with teaching timeslots"
                   };
        }
        public static ResultException ExamNotUpdate()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }016",
                       Message = "Unable to Update Examination"
                   };
        }

        public static ResultException ExamNotUpdateFinalDisabled()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }017",
                       Message = "Unable to Update Examination, Final disabled"
                   };
        }
        public static ResultException ExamNotUpdateMidtermDisabled()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }018",
                       Message = "Unable to Update Examination, Midterm disabled"
                   };
        }
        public static ResultException BuildingIsNotAvailable()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }019",
                       Message = "Unable to Update; This building is not available."
                   };
        }
    }
}