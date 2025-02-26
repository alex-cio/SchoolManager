using Microsoft.AspNetCore.Mvc;
using SchoolManager.Models;
using SchoolManager.Models.Db;
using SchoolManager.Models.Diff;
using SchoolManager.Services;

namespace SchoolManager.Api.Controllers;

[ApiController]
[Route("state")]
public class UnifiedApiController(IPupilClassManager pupilClassManager) : ControllerBase
{
    private readonly IPupilClassManager _pupilClassManager = pupilClassManager;


    #region Controllers
    /// <summary>
    /// GET /state - Retrieves the current state of pupils and classes.
    /// Retrieve data from each microservice with http calls  - include poly to retry calls if something went wrong when calling classes and/or 
    /// pupils get endpoints
    /// 
    /// </summary>
    [HttpGet]
    public IActionResult GetState()
    {
        return Ok(GetInitialState());
    }

    ///<summary>
    /// PATCH /state - Updates the class assignments of pupils.
    ///  We can ensure data consistency in multiple ways.
    ///  Synchronous Approach with Rollback - One way would be by calling each microservice
    ///  PATCH endpoint and if the first one fails, return to client an error. If the first update worked(pupilsUpdated)
    ///  and the second one (clasessUpdated) didn't, then we need to have in place a retry mechanism and a compensate mechanism
    ///  to try to rollback the first update.
    ///  Event-Driven Eventual Consistency - Another way would be to use eventual consistency and event driven design to send update events to each microservice
    ///  and if something happens to retry until data is consistent or compensate(ex. dead letter queues). That would involve displaying a in progress status or multiple statuses in the frontend,
    ///  maybe keeping the update states in a common storage(united api db, redis) and each microservice to notify frontend and united api at
    ///  event completion. 
    ///</summary>
    [HttpPatch]
    public IActionResult UpdateState([FromBody] Request assignments)
    {
        //calculate new state
      var newState = _pupilClassManager.UpdatePupilClassDivision(GetInitialState(), assignments);
      var diff = _pupilClassManager.Diff(GetInitialState(), newState);
      
      //to do - send update event from difference for each microservice or http call with update per microservice. 
      return Ok(new { message = "Pupils assigned successfully", newState, updatedPupils = diff.Item1, updatedClasses = diff.Item2  });
    }
#endregion

#region Privatemethods

/// <summary>
/// For testing purposes only - initial assignment - this data should be retrieved from each specific microservice
/// </summary>
/// <returns></returns>
private static State GetInitialState()
{
    return  new State()
        {
            Pupils = new List<Pupil>() {
                new Pupil() {
                    Id = 1,
                    Name = "Vermaercke Tim",
                },
                new Pupil() {
                    Id = 2,
                    Name = "Portauw Pieter"
                },
                new Pupil() {
                    Id = 3,
                    Name = "Maekelbergh Thibault",
                },
                new Pupil() {
                    Id = 4,
                    Name = "Petrescu Adrian-Mihai"
                },
                new Pupil() {
                    Id = 5,
                    Name = "De Vos Andres"
                },
                new Pupil() {
                    Id = 6,
                    Name = "Demaecker Caro",
                },
                new Pupil() {
                    Id = 7,
                    Name = "Goderis Jonas"
                },
                new Pupil() {
                    Id = 8,
                    Name = "Huyghe Lowie"
                },
                new Pupil () {
                    Id = 9,
                    Name = "Cornille Lukas"
                },
                new Pupil () {
                    Id = 10,
                    Name = "Nanescu Maria"
                },
                new Pupil () {
                    Id = 11,
                    Name = "Lasseel Siem"
                },
                new Pupil() {
                    Id = 12,
                    Name = "Spanhove Stijn"
                },
                new Pupil() {
                    Id = 13,
                    Name = "Verween Stijn"
                },
                new Pupil() {
                    Id = 14,
                    Name = "Dekiere Thomas"
                },
                new Pupil() {
                    Id = 15,
                    Name = "Akin Özgür"
                }
            },
            Classes = new List<Class>() {
                new Class() {
                    Id = 1,
                    ClassName = "First grade",
                    TeacherName = "Mr. Lemaire Jeroen",
                    MaxAmountOfPupils = 5,
                },
                new Class() {
                    Id = 2,
                    ClassName = "Second grade",
                    TeacherName = "Mr. Verbist Frank",
                    MaxAmountOfPupils = 20,
                }
            }
        };
}
#endregion
}
