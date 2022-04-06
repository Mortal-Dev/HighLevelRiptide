public class SprintingState : GroundStateUtil, IState 
{
    public IState Update(PlayerController playerController)
    {
        if (playerController.localPlayerInputInfo.IsPressingSprint)
            return Move(playerController.sprintSpeed, playerController, this);

        return new WalkingState().Update(playerController);
    }
}