namespace KeystoneLibrary.Exceptions
{
    public static class StudentAPIException
    {
        private static readonly string KEY = "API";
        public static ResultException AcademicLevelIdInvalid()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Academic Level Invalid"
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

        public static ResultException StudentsNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }003",
                       Message = "Students Not Found"
                   };
        }

        public static ResultException AdmissionRoundNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Admission Round Not Found"
                   };
        }

        public static ResultException StudentGroupNotFound(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }005",
                       Message = $"Student group with given code ({code}) is not found."
                   };
        }

        public static ResultException AdmissionTypeNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }006",
                       Message = $"Admission type with given name ({name}) is not found."
                   };
        }

        public static ResultException ResidentTypeNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }007",
                       Message = $"Resident type with given name ({name}) is not found."
                   };
        }

        public static ResultException StudentFeeTypeNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }008",
                       Message = $"Student fee type with given name ({name}) is not found."
                   };
        }

        public static ResultException StudentFeeGroupNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }009",
                       Message = $"Student fee group with given name ({name}) is not found."
                   };
        }

        public static ResultException TermNotFound(int year, int term)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }010",
                       Message = $"Term with given year ({year}) and term ({term}) is not found."
                   };
        }

        public static ResultException CurriculumVersionNotFound(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }011",
                       Message = $"Curriculum version with given code ({code}) is not found."
                   };
        }

        public static ResultException TitleNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }012",
                       Message = $"Title with given name ({name}) is not found."
                   };
        }

        public static ResultException GenderNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }013",
                       Message = $"Gender with given name ({name}) is not found."
                   };
        }

        public static ResultException NationalityNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }014",
                       Message = $"Nationality with given name ({name}) is not found."
                   };
        }

        public static ResultException RaceNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }015",
                       Message = $"Race with given name ({name}) is not found."
                   };
        }

        public static ResultException SpecializationGroupNotFound(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }016",
                       Message = $"Specialization Group with given code ({code}) is not found."
                   };
        }

        public static ResultException MaritalStatusGroupNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }017",
                       Message = $"Marital status with given name ({name}) is not found."
                   };
        }

        public static ResultException CountryNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }018",
                       Message = $"Country with given name ({name}) is not found."
                   };
        }

        public static ResultException ProvinceNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }019",
                       Message = $"Province with given name ({name}) is not found."
                   };
        }

        public static ResultException DistrictNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }020",
                       Message = $"District with given name ({name}) is not found."
                   };
        }

        public static ResultException SubdistrictNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }021",
                       Message = $"Subdistrict with given name ({name}) is not found."
                   };
        }

        public static ResultException InvalidStudentCode(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }022",
                       Message = $"Given student code ({code}) is invalid."
                   };
        }

        public static ResultException DuplicateStudentCode(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }023",
                       Message = $"Given student code ({code}) is already exists."
                   };
        }

        public static ResultException InvalidTermFormat(string academicYearTerm)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }024",
                       Message = $"Given academic year and term ({academicYearTerm}) is invalid."
                   };
        }

        public static ResultException AcademicLevelNotFound(string code)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }025",
                       Message = $"Academic level with given code ({code}) is not found."
                   };
        }

        public static ResultException RelationshipNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }026",
                       Message = $"Relationship with given name ({name}) is not found."
                   };
        }

        public static ResultException RegistrationStatusNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }027",
                       Message = $"Registration status with given name ({name}) is not found."
                   };
        }

        public static ResultException AcademicProgramNotFound(string name)
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }028",
                       Message = $"Academic program with given name ({name}) is not found."
                   };
        }

        public static ResultException DateWrongFormat(string date)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}029",
                Message = $"Given Date ({date}) is invalid. Format = (dd/mm/yyyy)"
            };
        }

        public static ResultException StudentInActive()
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}030",
                Message = $"Student is inactive."
            };
        }

        public static ResultException StateNotFound(string name)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}031",
                Message = $"State with given name ({name}) is not found."
            };
        }

        public static ResultException CityNotFound(string name)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}032",
                Message = $"City with given name ({name}) is not found."
            };
        }

        public static ResultException ReligionNotFound(string name)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}033",
                Message = $"Religion with given name ({name}) is not found."
            };
        }

        public static ResultException InvalidAddressType(string input)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}034",
                Message = $"Input Address type with ({input}) is invalid. Possible value is ['c' = current address, 'p' = permanent address]"
            };
        }

        public static ResultException InvalidStudentStatus(string input)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}035",
                Message = $"Input Student status ({input}) is invalid."
            };
        }

        public static ResultException NativeLanguageNotFound(string input)
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}036",
                Message = $"Input native Language ({input}) is not found."
            };
        }
    }
}