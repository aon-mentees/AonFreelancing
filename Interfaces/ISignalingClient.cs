namespace AonFreelancing.Interfaces;

public interface ISignalingClient
{
    public Task GetOffer(string offerJson);
    public Task GetAnswer(string answerJson);
    public Task GetIceCandidate(string iceCandidateJson);
}