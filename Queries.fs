module Queries

//Disable warnings for the match below
#nowarn "49"
#nowarn "26"

let param, column =
    match Database.chosenDb with 
    | Oracle -> ":", "\""
    | MySQL -> "@", "`"

let DELETE_MESSAGE_SOURCES = "DELETE FROM MessageSources"
let DELETE_SOURCES = "DELETE FROM Sources"
let DELETE_MESSAGES = "DELETE FROM Messages"

let INSERT_MESSAGE = "INSERT INTO Messages VALUES ("+param+"Id, "+param+"PTime, "+param+"Msg, "+param+"PLevel)"
let INSERT_SOURCE = "INSERT INTO Sources VALUES ("+param+"Id, "+param+"Src)"
let INSERT_MESSAGE_SOURCE = "INSERT INTO MessageSources VALUES ("+param+"Id, "+param+"MsgId, "+param+"SourceId)"

let SELECT_SOURCE_ID = "SELECT id FROM Sources WHERE source = "+param+"Src"
let SELECT_MESSAGES = 
        "SELECT Messages.id, "+column+"TIME"+column+", msg, "+column+"level"+column+", source
         FROM Messages
         LEFT JOIN MessageSources ON (Messages.id = MessageSources.msg_id)
         LEFT JOIN Sources ON (Sources.id = MessageSources.source_id)"

let ORDERED = "
         ORDER BY Messages.id"

let SELECT_MESSAGES_WHERE_LEVEL sign = SELECT_MESSAGES + "WHERE "+column+"level"+column+" "+sign+" "+param+"PLevel"+ORDERED

let SELECT_SOURCES = "SELECT source FROM Sources"

let SELECT_MESSAGES_WHERE_SOURCE = SELECT_MESSAGES + "WHERE source = "+param+"Src" + ORDERED

let SELECT_KERNEL_VERSION = SELECT_MESSAGES + "WHERE msg like 'Linux version%'" + ORDERED

let SELECT_NUMBER_OF_MESSAGES = "SELECT COUNT(*) FROM Messages"

let SELECT_NUMBER_OF_MESSAGES_BY_PRIORITY = "SELECT "+column+"level"+column+" as Priority, COUNT(*) as Count FROM Messages GROUP BY "+column+"level"+column

let SELECT_NUMBER_OF_SOURCES = "SELECT COUNT(*) FROM Sources"

let SELECT_SOURCES_ORDERED_BY_MESSAGE_COUNT = 
        "SELECT source FROM Sources 
         JOIN MessageSources ON (Sources.id = MessageSources.source_id)
         GROUP BY source
         ORDER BY COUNT(*) DESC"

let SELECT_SOURCES_WITH_ERRORS_ORDERED_BY_MESSAGE_COUNT =
        "SELECT source FROM Sources
         JOIN MessageSources ON (Sources.id = MessageSources.source_id)
         LEFT JOIN Messages ON (Messages.id = MessageSources.msg_id)
         WHERE Messages."+column+"level"+column+" > 4
         GROUP BY source
         ORDER BY COUNT(*) DESC"
