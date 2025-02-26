using SchoolManager.Models;
using SchoolManager.Models.Db;
using SchoolManager.Models.Diff;
using System.Linq;

namespace SchoolManager.Services;

/**
 * This class manages the division of pupils into classes.
 *
 * It's your job to implement the UpdatePupilClassDivision method and the Diff method.
 */

public class PupilClassManager : IPupilClassManager
{
 public  State UpdatePupilClassDivision(State state, Request request)
{
    // Clone the initial state to avoid modifying the original reference
    state = Clone(state);

    // Validate 
    ValidateAssignements(state, request);

    // Process each assignment in the request
    ProcessAssignments(state, request);

    return state;
}
 
public  (List<UpdatedPupil>, List<UpdatedClass>) Diff(State oldState, State newState)
{
    // Lists to store updated pupils and classes
    var updatedPupils = new List<UpdatedPupil>();
    var updatedClasses = new List<UpdatedClass>();

    // Compare each pupil from the new state with the old state
    foreach (var pupil in newState.Pupils)
    {
        var oldPupil = oldState.Pupils.First(p => p.Id == pupil.Id);

        // Check if the pupil's class or follow-up number has changed
        if (oldPupil.ClassName != pupil.ClassName || oldPupil.FollowUpNumber != pupil.FollowUpNumber)
        {
            updatedPupils.Add(new UpdatedPupil
            {
                PupilId = pupil.Id,
                ClassName = pupil.ClassName,
                FollowUpNumber = pupil.FollowUpNumber
            });
        }
    }

    // Compare each class from the new state with the old state
    foreach (var @class in newState.Classes)
    {
        var oldClass = oldState.Classes.First(c => c.Id == @class.Id);

        // Check if the number of pupils in the class has changed
        if (oldClass.AmountOfPupils != @class.AmountOfPupils)
        {
            updatedClasses.Add(new UpdatedClass
            {
                ClassId = @class.Id,
                AmountOfPupils = @class.AmountOfPupils
            });
        }
    }

    // Return the lists of updated pupils and classes
    return (updatedPupils, updatedClasses);
}

#region Private members
private void ProcessAssignments(State state, Request request)
{
    foreach (var assignment in request.Assignments)
    {
        var newClass = state.Classes.First(c => c.Id == assignment.ClassId);
        var pupil = state.Pupils.First(p => p.Id == assignment.PupilId);

        // Check if the pupil is changing classes
        if (pupil.ClassName != newClass.ClassName)
        {
            // Increase the number of pupils in the new class
            newClass.AmountOfPupils++;

            // Validate that the new class does not exceed its max capacity
            if (newClass.AmountOfPupils > newClass.MaxAmountOfPupils)
            {
                throw new Exception($"Class {newClass.ClassName} has too many pupils assigned.");
            }

            // If the pupil was previously assigned to a class, update the old class
            var oldClass = state.Classes.FirstOrDefault(c => c.ClassName == pupil.ClassName);
            if (oldClass != null)
            {
                oldClass.AmountOfPupils--;

                // Adjust follow-up numbers for pupils in the old class
                var updatePupilsFollowup = state.Pupils.Where(o => o.ClassName == oldClass.ClassName).ToList();
                foreach (var updatePupil in updatePupilsFollowup)
                {
                    if (pupil.FollowUpNumber < updatePupil.FollowUpNumber)
                    {
                        updatePupil.FollowUpNumber--;
                    }
                }
            }

            // Update pupil's class and reset follow-up number
            pupil.ClassName = newClass.ClassName;
            pupil.FollowUpNumber = 1;

            // Adjust follow-up numbers for pupils in the new class
            var updatePupilsFollowup2 = state.Pupils.Where(o => o.ClassName == newClass.ClassName).ToList();
            foreach (var updatePupil in updatePupilsFollowup2)
            {
                if (pupil.Id != updatePupil.Id)
                {
                    updatePupil.FollowUpNumber++;
                }
            }
        }
    }
}

private void ValidateAssignements(State state, Request request)
{
    // Validate that all requested class assignments exist in the state
    var nonExistentClass = request.Assignments.Select(o => o.ClassId)
        .Except(state.Classes.Select(o => o.Id)).ToList();
    if (nonExistentClass.Count > 0)
    {
        throw new Exception($"Class with id {nonExistentClass.First()} does not exist.");
    }

    // Validate that all pupils in the request exist in the state
    var nonExistentPupils = request.Assignments.Select(o => o.PupilId)
        .Except(state.Pupils.Select(o => o.Id)).ToList();
    if (nonExistentPupils.Count > 0)
    {
        throw new Exception($"Pupil with id {nonExistentPupils.First()} does not exist.");
    }

    // Ensure there are no duplicate pupil assignments in the request
    var duplicatePupils = request.Assignments.GroupBy(o => o.PupilId)
        .Where(o => o.Count() > 1).Select(o => o.Key).ToList();
    if (duplicatePupils.Count > 0)
    {
        throw new Exception($"Duplicate pupil IDs provided.");
    }

    // Validate that all unassigned pupils are being assigned to a class
    var pupilsNotAssigned = state.Pupils.Where(o => string.IsNullOrEmpty(o.ClassName))
        .Select(o => o.Id).ToList();
    if (pupilsNotAssigned.Except(request.Assignments.Select(o => o.PupilId)).Any())
    {
        throw new Exception($"Pupil with id {pupilsNotAssigned.First()} is not assigned to a class.");
    }
}

private State Clone(State state)
{
    state = new State()
    {
        Classes = state.Classes.Select(o => new Class
        {
            Id = o.Id,
            ClassName = o.ClassName,
            AmountOfPupils = o.AmountOfPupils,
            MaxAmountOfPupils = o.MaxAmountOfPupils,
            TeacherName = o.TeacherName
        }).ToList(),
        Pupils = state.Pupils.Select(o => new Pupil
        {
            Id = o.Id,
            ClassName = o.ClassName,
            FollowUpNumber = o.FollowUpNumber,
            Name = o.Name
        }).ToList()
    };
    return state;
}
#endregion
}
