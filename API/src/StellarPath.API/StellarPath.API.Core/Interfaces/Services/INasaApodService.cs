using System;
using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Services
{
  public interface INasaApodService
  {
    Task<NasaApodResponse?> GetPictureOfTheDayAsync(string apiKey);
  }
}
