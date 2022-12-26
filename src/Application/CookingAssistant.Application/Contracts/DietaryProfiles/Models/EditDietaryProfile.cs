using Application.Domain.CookingAssistant;
using AutoMapper;
using CookingAssistant.Application.Mappings;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.DietaryProfiles.Models;

public class EditDietaryProfile : IMapFrom<DietaryProfile>
{
    public string Birthday { get; set; }
    public string Gender { get; set; }
    public short? HeightCm { get; set; }
    public short? HeightFeet { get; set; }
    public short? HeightInches { get; set; }
    public float? WeightKg { get; set; }
    public short? WeightLbs { get; set; }
    public string ActivityLevel { get; set; }
    public string Goal { get; set; }
    public DailyIntake DailyIntake { get; set; } = new DailyIntake();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<DietaryProfile, EditDietaryProfile>()
            .ForMember(x => x.Birthday, opt => opt.MapFrom(src => src.Birthday.Value.ToString("yyyy-MM-dd")))
            .ForMember(x => x.HeightCm, opt => opt.MapFrom<HeightCmResolver>())
            .ForMember(x => x.HeightFeet, opt => opt.MapFrom<HeightFeetResolver>())
            .ForMember(x => x.HeightInches, opt => opt.MapFrom<HeightInchesResolver>())
            .ForMember(x => x.WeightKg, opt => opt.MapFrom<WeightKgResolver>())
            .ForMember(x => x.WeightLbs, opt => opt.MapFrom<WeightLbsResolver>())
            .ForMember(x => x.DailyIntake, opt => opt.Ignore());
    }
}