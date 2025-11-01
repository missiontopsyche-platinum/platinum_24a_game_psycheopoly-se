flowchart LR

&nbsp;   A\[Main Menu] -->|Start Game| B\[Game Board]

&nbsp;   A -->|Settings| E\[Settings Menu]

&nbsp;   E -->|Save and Back| A



&nbsp;   B -->|Open Pause Menu / ESC| C\[Pause Menu]

&nbsp;   C -->|Resume| B

&nbsp;   C -->|Quit to Menu| A



&nbsp;   B -->|Player Lands on Property| D\[Property Pop up]

&nbsp;   D -->|Buy Property| B

&nbsp;   D -->|Cancel| B



&nbsp;   B -->|End Turn| F\[Turn Banner Transition]

&nbsp;   F -->|Next Player Ready| B



