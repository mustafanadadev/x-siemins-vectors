namespace x_siemens_task.BLL
{
    public interface IVectorOperations
    {
        List<KeyValuePair<int, double>> FindTop5MostSimilarFor(int id);
    }
}
