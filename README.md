# SurveySolutionsAPI

Forked from example code provided by Survey Solutions here:
http://support.mysurvey.solutions/customer/en/portal/articles/2574862-api-for-data-export?b_id=12728

Hard coded config has been swapped out, for a config read in from json file `.\SurveySolutions.config`:

    {
        "server": "https://XXXXXXXXXXXXXXXXXXXXXXXXXXX",
        "apiLogin": "XXXXXXXXXXXXXXXXXXXXXXXXXXX",
        "apiPassword": "XXXXXXXXXXXXXXXXXXXXXXXXXXX",
        "fmt": "tabular",
        "surveys": {
          "TA": {
              "template": "d5054668-eb70-4463-9c35-df063f47069f",
              "version": "1"
          },
          "SLSS": {
              "template": "587ff4eb-611a-45ae-8f89-6baea1d08afa",
              "version": "1"
          }
        }
    }

`GetExport.exe` expects one command line argument, which must match one of the keys of the object under `"surveys"` in the config file.  i.e., with the config above, either `TA` or `SLSS` 

The downloaded zipfile is output to stdout, which can be piped into another utility to process the data, or redirected to save as a file:

`GetExport.exe TA > TeachersAssessments.zip`
