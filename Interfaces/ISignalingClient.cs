namespace AonFreelancing.Interfaces;

public interface ISignalingClient
{
    public Task GetOffer(long senderUserId, string offerJson);
    public Task GetAnswer(long senderUserId, string answerJson);
    public Task GetIceCandidate(long senderUserId, string iceCandidateJson);
}