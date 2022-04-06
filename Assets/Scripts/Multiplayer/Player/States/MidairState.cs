public class MidairState : IState
{
    public IState Update(PlayerController playerController)
    {
        //if (playerController.localPlayerInputInfo.IsPressingForwards) playerController.MoveForward()
        return this;
    }
}
