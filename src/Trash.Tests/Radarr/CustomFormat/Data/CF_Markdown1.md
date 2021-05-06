
<sub><sub><sub>Score [480]</sub>

??? example "json"

    ```json
    {
        "trash_id": "4eb3c272d48db8ab43c2c85283b69744",
        "name": "DTS-HD/DTS:X",
        "includeCustomFormatWhenRenaming": false,
        "specifications": [{
            "name": "dts.?(hd|es|x(?!\\d))",
            "implementation": "ReleaseTitleSpecification",
            "negate": false,
            "required": false,
            "fields": {
                "value": "dts.?(hd|es|x(?!\\d))"
            }
        }]
    }
    ```

<sub><sup>[TOP](#index)</sup>

------

### Surround Sound

>If you prefer all kind of surround sounds

!!! warning

    Don't use this Custom Format in combination with the `Audio Advanced` CF if you want to fine tune your audio formats or else it will add up the scores.


<sub><sub><sub>Score [500]</sub>

??? example "json"

    ```json
    {
        "trash_id": "43bb5f09c79641e7a22e48d440bd8868",
        "name": "Surround Sound",
        "includeCustomFormatWhenRenaming": false,
        "specifications": [{
            "name": "dts\\-?(hd|x)|truehd|atmos|dd(\\+|p)(5|7)",
            "implementation": "ReleaseTitleSpecification",
            "negate": false,
            "required": false,
            "fields": {
                "value": "dts\\-?(hd|x)|truehd|atmos|dd(\\+|p)(5|7)"
            }
        }]
    }
    ```

    ```json
    {
        "trash_id": "abc",
        "name": "No Score"
    }
    ```

    ```json
    {
        "trash_id": "xyz",
        "name": "One that won't be in config"
    }
    ```


