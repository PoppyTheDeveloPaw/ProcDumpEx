# ProcDumpEx

ProcDumpEx extends ProcDump with additional functionality, such as process monitoring and /or simplified parameter input.  
By pressing the key combinations CTRL+C, CTRL+Break or the X key, ProcDumpEx can be terminated at any time. All ProcDump instances currently started by ProcDumpEx are thereby terminated. ProcDumpEx must be started as admin.

Inspired by https://github.com/tedyyu/ProcDumpEx and reinterpreted

##Necessary Files

| ProcDump file   | Resulting Procdump calls                            |
| --------------- | --------------------------------------------------- |
| procdump.exe    | `./procdump.exe` or `./Procdump/procdump.exe`       |
| procdump64.exe  | `./procdump64.exe` or `./Procdump/procdump64.exe`   |
| procdump64a.exe | `./procdump64a.exe` or `./Procdump/procdump64a.exe` |

If one of the files is missing ProcDumpEx starts with error.

## Download

The latest version can be downloaded at [ProcDumpEx](https://github.com/PoppyTheDeveloPaw/ProcDumpEx/raw/main/ProcDumpEx/ProcDumpEx.zip)

## Supported parameters

ProcDumpEx provides the following additional parameters:

### Parameter '-64'

ProcDump internally checks whether the process to be monitored is a 32- or 64-bit application and starts procdump.exe or procdump64.exe depending on the process. With the parameter -64, this check is bypassed and the 64-bit variant is always used.

#### Example:

| ProcDumpEx call                             | Resulting Procdump calls                   |
| ------------------------------------------- | ------------------------------------------ |
| `procdumpex.exe -64 -ma -e <process 1>.exe` | `procdump64.exe -ma -e <PID of process 1>` |

### Parameter '-cputhd'

The cputhd parameter extends the -c parameter of Procdump. It checks whether one or more processes have exceeded a certain CPU usage and then generates memory dumps. The cputhd parameter allows the user to set one or more values to monitor and generate memory dumps accordingly. It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of % (percent).

#### Example:

| ProcDumpEx call                                                            | Resulting Procdump calls                                                                                                                                                                                                                                                                                                                                                                          |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -cputhd "10,50,80" "<process 1>.exe, <process 2>.exe"` | `<procdump.exe/procdump64.exe> -ma -c 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -c 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -c 80 <PID of process 2>` |

### Parameter '-cputhdl'

Parameter cputhdl extends the -cl parameter of Procdump. This parameter checks if one or more processes have fallen below a certain CPU usage and then creates memory dumps. The cputhdl parameter allows the user to set one or more values to monitor and create memory dumps accordingly. It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of % (percent).

#### Example:

| ProcDumpEx call                                                             | Resulting Procdump calls                                                                                                                                                                                                                                                                                                                                                                                |
| --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -cputhdl "10,50,80" "<process 1>.exe, <process 2>.exe"` | `<procdump.exe/procdump64.exe> -ma -cl 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 80 <PID of process 2>` |

### Parameter '-help'

The -help parameter can be used to display explanations and examples for the individual parameters supported by ProcDumpEx. In addition, the help of ProcDump is printed.  
As soon as the parameter '-help' occurs in the specified arguments, nothing is executed but only the help of ProcDumpEx and ProcDump is displayed.

#### Example:

- `procdumpex.exe -help`

### Parameter '-inf'

The '-inf' parameter in ProcDumpEx ensures that new ProcDump instances are continuously opened until explicitly terminated. When combined with the '-n' parameter in ProcDump, this means that when the number of dump files specified with '-n' is reached, ProcDump will terminate, and ProcDumpEx will subsequently reopen ProcDump with the same parameters. However, this parameter can generate a high disk usage and its limit is determined by the overall size and speed of the system.

#### Example:

| ProcDumpEx call                                                        | Resulting Procdump calls                                                                                                |
| ---------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -w -e -inf -pn "<process 1>.exe, <process 2>.exe“` | `<procdump.exe/procdump64.exe> -ma -e <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -e <PID of process 2>` |

### Parameter '-memthd'

The 'memthd' parameter extends the ProcDump '-m' parameter. By using the 'memthd' parameter, users can set multiple memory thresholds for one or more processes without having to manually open ProcDump instances. It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of megabytes (MB).

#### Example:

| ProcDumpEx call                                                            | Resulting Procdump calls                                                                                                                                                                                                                                                                                                                                                                          |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -memthd "10,50,80" "<process 1>.exe, <process 2>.exe"` | `<procdump.exe/procdump64.exe> -ma -m 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -m 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -m 80 <PID of process 2>` |

### Parameter '-memthdl'

The '-memthdl' parameter extends the existing ProcDump '-ml' parameter. It allows users to set lower memory thresholds for one or more processes such that if one or more processes falls below the specified memory usage, a memory dump will be generated. It is also possible to combine this parameter with the '-pn' and '-inf' parameters. The values provided are expressed in units of megabytes (MB).

#### Example:

| ProcDumpEx call                                                             | Resulting Procdump calls                                                                                                                                                                                                                                                                                                                                                                                |
| --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -memthdl "10,50,80" "<process 1>.exe, <process 2>.exe"` | `<procdump.exe/procdump64.exe> -ma -ml 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 80 <PID of process 2>` |

### Parameter '-pn'

If the parameter '-pn' is used with a process name, ProcDumpEx retrieves the process IDs (PIDs) of all programmes and their corresponding subordinate processes with the same name and opens a separate ProcDump instance for each of them. It is also possible to directly specify the ID of a process instead of its name. In connection with the parameter '-inf', however, this can only be restarted as long as the process with the specified ID exists.

The parameter '-pn' is an optional parameter, as it is also possible to specify the process names and process IDs at the end of the arguments.  
If it is necessary that the PID is inserted at a certain position and not at the end of the arguments, this can be achieved with the placeholder [ProcessPlaceholder].

#### Examples:

| ProcDumpEx call                                                                                                                                                                         | Resulting Procdump calls                                                                                                |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `ProcDumpEx.exe -ma -e -pn <process 1>.exe>`<br/>**alternative:** `ProcDumpEx.exe -ma -e <process 1>.exe>`                                                                              | `<procdump.exe/procdump64.exe> -ma -e <PID of process 1>`                                                               |
| `ProcDumpEx.exe -ma -e -pn <PID 1>`<br/>**alternative:** `ProcDumpEx.exe -ma -e <PID 1>`                                                                                                | `<procdump.exe/procdump64.exe> -ma -e <PID 1>`                                                                          |
| `ProcDumpEx.exe -ma -e -pn "<process 1>.exe, <process 2>.exe"`<br/>**alternative:** `ProcDumpEx.exe -ma -e "<process 1>.exe, <process 2>.exe"`                                          | `<procdump.exe/procdump64.exe> -ma -e <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -e <PID of process 2>` |
| `ProcDumpEx.exe -ma -e -pn "<PID 1>, <PID 2>"`<br/>**alternative:** `ProcDumpEx.exe -ma -e "<PID 1>, <PID 2>"`                                                                          | `<procdump.exe/procdump64.exe> -ma -e <PID 1>`<br/>`<procdump.exe/procdump64.exe> -ma -e <PID 2>`                       |
| `ProcDumpEx.exe -ma -e -pn "<PID 1>, <process 2>.exe"`<br/>**alternative:** `ProcDumpEx.exe -ma -e "<PID 1>, <process 2>.exe"`                                                          | `<procdump.exe/procdump64.exe> -ma -e <PID 1>`<br/>`<procdump.exe/procdump64.exe> -ma -e <PID of process 2>`            |
| `ProcDumpEx.exe -ma [ProcessPlaceholder] -e -pn "<process 1>.exe, <process 2>.exe"`<br/>**alternative:** `ProcDumpEx.exe -ma [ProcessPlaceholder] -e "<process 1>.exe, <process 2>.exe` | `<procdump.exe/procdump64.exe> -ma <PID of process 1> -e`<br/>`<procdump.exe/procdump64.exe> -ma <PID of process 2> -e` |

### Parameter '-showoutput'

Since ProcDumpEx starts several ProcDump instances at the same time, the normal output of ProcDump is suppressed and only displayed when a ProcDump instance has been started or terminated. With the parameter -showoutput, the output of the ProcDump instances is displayed after they have been terminated.

#### Examples:

| ProcDumpEx call                                     | Resulting Procdump calls                                  |
| --------------------------------------------------- | --------------------------------------------------------- |
| `procdumpex.exe -showoutput -ma -e <process 1>.exe` | `<procdump.exe/procdump64.exe> -ma -e <PID of process 1>` |

### Parameter '-w'

The '-w' parameter is a parameter used by ProcDump itself. When used in conjunction with ProcDumpEx, it allows ProcDumpEx to detect the termination and restart of a process, and re-open ProcDump instances for the processes found using the '-pn' parameter and process name. This ensures that the desired processes are constantly monitored even after termination and restart.

#### Examples:

| ProcDumpEx call                                                   | Resulting Procdump calls                                                                                                |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -w -e -pn "<process 1>.exe, <process 2>.exe“` | `<procdump.exe/procdump64.exe> -ma -e <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -e <PID of process 2>` |

## Hint

It is also possible to combine the parameters '-memthd', '-memthdl', '-cputhd' and '-cputhdl', which would lead to the following calls

| ProcDumpEx call                                                                                                                       | Resulting Procdump calls                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `procdumpex.exe -ma -cputhd "10,50,80" -cputhdl "10,50,80" -memthd "10,50,80" -memthdl "10,50,80" "<process 1>.exe, <process 2>.exe"` | `<procdump.exe/procdump64.exe> -ma -c 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -c 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -c 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -c 80 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -cl 80 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -m 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -m 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -m 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -m 80 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 10 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 50 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 80 <PID of process 1>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 10 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 50 <PID of process 2>`<br/>`<procdump.exe/procdump64.exe> -ma -ml 80 <PID of process 2>` |
