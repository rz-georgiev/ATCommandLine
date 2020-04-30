# ATCommandLine\
Sending commands to a SIM modem Siemens M20 / M20 Terminal\

How to use:\
First all possible COM ports and their settings show on the screen.\
Enter a value for the COM port and all the other settings.\
If you don't enter anything for a specific setting, the default parameter showed in the parantheses is used. Example: "BaudRate(9600): "\

We have integrated the most basic commands as call and sendSms + a custom command to do something else \
Possible commands after the setup:\
*Call eg -> call [mobileNumber] -> call 1234567890\
*Sms eg -> sendSms [mobileNumber] [message] -> sendSms 1234567890 someMessage\
*Custom command -> custom [command] -> custom AT+VGR\
*Closing the program -> exit \

Link to Siemens documentation:\
https://canarysystems.com/nsupport/m20_v5.pdf\
