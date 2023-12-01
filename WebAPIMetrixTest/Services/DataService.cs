namespace WebAPIMetrixTest.Services
{
    public class DataService
    {
        public int GetRandomDataNumber()
        {
            return Random.Shared.Next(0, 100);
        }
    }
}