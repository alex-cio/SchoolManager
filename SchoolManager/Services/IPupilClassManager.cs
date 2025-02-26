namespace SchoolManager.Services;
using SchoolManager.Models;
using SchoolManager.Models.Diff;

public interface IPupilClassManager
{
    State UpdatePupilClassDivision(State state, Request request);

    (List<UpdatedPupil>, List<UpdatedClass>) Diff(State oldState, State newState);
}
