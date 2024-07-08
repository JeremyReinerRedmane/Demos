using mCASE_ADMIN.DataAccess.mCase;//DemoKatan.mCase;

var commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length > 1)
{
    if (commandLineArgs.Length == 8)
    {
        //directly querying db for datalist id's
        var cmd = new SyncDlConfigs(commandLineArgs);

        var sqlQuery = cmd.DataAccess().Result;

        cmd.RemoteSync(sqlQuery);
    }

    if (commandLineArgs.Length == 7)
    {
        //direct access to csv data
        var cmd = new SyncDlConfigs(commandLineArgs);

        var data = cmd.DirectDataAccess();

        cmd.RemoteSync(data);
    }

}
else
{
    var cmd = new List<string>
    {

    }.ToArray();

    var local = new SyncDlConfigs(cmd);
    if (cmd.Length == 8)
    {
        //directly querying db
        var sqlQuery = await local.DataAccess();

        local.RemoteSync(sqlQuery);
    }

    if (cmd.Length == 7)
    {
        //direct access to csv data
        var data = local.DirectDataAccess();

        local.RemoteSync(data);
    }
}

/* TASKS
 * ---------------------------------------------------------Required---------------------------------------------------------
 * 1  []: Need to export usings to cmd param for project specific needs (Main file usings, and Single static usings)
 * 1B:    This also requires fixing the constructor from 7 and 8 params to potentially 9-10 params (two additional params for 1 for static file, and the second for class file)
 *
 * 2  []: Need to convert default data structures to nullable types (Datetime,... Any others?))
 *
 * 3  []: Boolean can remain string, but add to static file boolean extension that converts string to true boolean value
 * 3A:    boolean can be one of three values. true, false, null
 * 3B:    boolean string can be one of mCase constant true values, or mcase constant false values. Not just true or false.
 * 3C:    if string boolean value is not found in mCase constant true/false values return one of global errors? / save as empty string?
 *
 * 4  []: Continue with mandatory conditional values
 * 4A:    Conditionally mandatory required fields are waiting for clarity on how the json string passes data. (currently using system name of field. Could be fieldId instead)
 *
 * 5  []: double back and verify all conditional values
 *
 * 6  []: validate that for All data structures default values are properly checked and received. (currently string with default value ':)' will throw exception)
 *
 * 7  []: Add auto logs to constructor for creation of entity, saving for saving the entity, and deleting for deleting the entity
 * 7B:    Need to add deleting method for soft deleting record
 *
 * 8  []: Convert all string returns to string builder. Update the string builder to receive string builder as argument in place of string.
 *
 * 9  []: Save record should return list of required fields. Update the CanSave() -> CheckRequiredFields(). And save method if should return required fields (List<strings>())
 *
 * 10 []: Stored value types need to reflect accurate null values if presented. String.Empty is a accepted null value to store in DB as no value was presented.
 * 10A:    potential nullable value types: bool, datetime, time, number....?
 *
 * 11 []: Use parent child relationships to recieve child records based off of listId, (not just embedded records). List of records should return value type
 * --------------------------------------------------------- Syntactic Sugar ---------------------------------------------------------
 *
 * 1 []: Move logging into methods region
 *
 * 2 []: Should systemName strings refer to system name mappers?
 *
 * 3 []: Update doc tags for accurate descriptions
 *
 * 4 []: How do we reflect readonly / mirrored fields, is this system currently accurate? Where do we use only getters vs gets && sets
 *
 * 5 []: need to build static datetime comparison method for determining datetime greater than less then, and datetime ordering for ICollections, one to ones
 *
 * 6 []: Add error validation global errors to class constructor for more concise error referencing across fields / methods
 *
 * 7 []: Return docstrings to static methods for clarity on return type on actual methods in static extensions as well as doc strings on class instance method calls
 */