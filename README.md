
# Scripts Explanation 
# AppleSc.cs

`AppleSc` is a Unity script that enables interaction with apples using the XR Interaction Toolkit. The script tracks when an apple is grabbed or released, measuring the time it was held and disabling gravity during the interaction.

---

## 🎯 Purpose

To manage interactions with apples, track how long they are held, and disable gravity while they are being grabbed to simulate natural hand interactions in VR.

---

## 🔧 Dependencies

- **XR Interaction Toolkit**: Required for grab and release interactions.

---

## ⚙️ Functionality

### `Start()`
- Initializes the XRGrabInteractable component and Rigidbody.
- Adds listeners for grab (`selectEntered`) and release (`selectExited`) events.

### `Update()`
- Continuously tracks the time the apple is held if it is being interacted with.

### `OnGrab(SelectEnterEventArgs args)`
- Sets the apple as being held, resets the hold time, and disables gravity to simulate natural grabbing behavior.

### `OnRelease(SelectExitEventArgs args)`
- Marks the apple as not being held, restores gravity, and logs the total time the apple was held.

---

## 🧠 Interaction Flow

1. **Grab**: When the user grabs the apple, the apple's gravity is disabled, and the hold time begins to count.
2. **Release**: When the apple is released, gravity is re-enabled, and the total hold time is logged.

---

## 🔍 Example Log

```
Elma tutuldu.
Elma bırakıldı. Tutma süresi: 2.45 saniye
```

---


# HastaVeriYoneticisi.cs (Unity + Firebase)

The `HastaVeriYoneticisi` (Patient Data Manager) is a Unity script designed to interact with **Firebase Firestore**, fetching gameplay settings and spawning apples based on patient-specific records. It’s primarily used in therapeutic or evaluation games where personalized data is essential.

---

## 🚀 Features

- Connects to Firebase Firestore.
- Retrieves the latest game settings (`oyunAyarlari`) for a specified patient.
- Instantiates apples (normal or rotten) at saved positions based on recent records.
- Handles potential missing fields gracefully with default behaviors and debug warnings.

---

## 🔧 Requirements

- **Unity 2020+**
- **Firebase SDK for Unity**
- Firebase Firestore database structured as:
  ```
  /hastalar/{tcKimlikNo}/gunlukKayitlar/{autoId}/
    - oyunAyarlari.sure (int)
    - elma1..5:
        - konum: {x, y, z}
        - normalElma: true/false
  ```
- Prefabs for:
  - Normal Apple (`applePrefab`)
  - Rotten Apple (`rottenApplePrefab`)

---

## 🧩 Public Fields

- `GameObject applePrefab`: Prefab for normal apples.
- `GameObject rottenApplePrefab`: Prefab for rotten apples.
- `string hedefHastaTcKimlikNo`: Target patient’s identity number (used to fetch their data).

---

## 🧠 Workflow

1. **Start Method**  
   - Initializes Firebase.
   - Calls:
     - `FetchOyunAyarlari()`: Retrieves game duration (`sure`) from latest daily record.
     - `LoadApplePositions()`: Spawns apples based on their saved locations and type.

2. **InitializeFirebase()**  
   - Ensures all Firebase dependencies are available before using Firestore.

3. **FetchOyunAyarlari(string tc)**  
   - Retrieves and logs the duration of the game for a specific patient.
   - If missing, defaults to 60 seconds and logs a warning.

4. **LoadApplePositions(string tc)**  
   - Loads last 5 apple spawn records from the latest daily document.
   - Determines apple type (`normalElma`) and spawns each at the recorded 3D position.

5. **SafeToFloat()**  
   - Helper function to safely convert stored values to `float`.

---

## 📌 Example Log Output

```
Firebase (Firestore) başarıyla başlatıldı.
Oyun Süresi: 45
elma1 spawn edildi: (1.2, 3.4, 2.1) - Normal
elma2 spawn edildi: (2.1, 1.0, 3.3) - Çürük
```

---

## 🛠️ Tips for Use

- Assign `applePrefab` and `rottenApplePrefab` via the Unity Inspector.
- Make sure Firestore security rules allow reads for your authenticated Unity client.
- Set `hedefHastaTcKimlikNo` dynamically if multiple patients are evaluated.

---

## 📁 Future Improvements

- Add retry mechanism for network failures.
- Expose game time and apple data to external managers (e.g. UI countdown or scoring system).
- Extend for more than 5 apples if needed.

---
# BadBasket.md

## 📋 Description
The `BadBasket` script is responsible for identifying if the apple thrown by the player has entered the incorrect basket. It is commonly used in gameplay mechanics where sorting apples correctly is part of the objective.

---

## ⚙️ How It Works
- The script checks collisions between the basket and the apples.
- If a bad apple enters this basket, it will be marked accordingly.
- Updates scores or logs the event.

---

## 🔗 Dependencies
- `UnityEngine`
- Tag-based collision detection

---

## 🛠️ Usage
1. Attach this script to the **"bad" basket** object.
2. Ensure the apples have identifiable tags (e.g., `"Apple"`).
3. Handle scoring logic or feedback when apples collide.

---

## ✅ Recommended Settings
- Collider component set to **trigger**.
- Apples must have `Rigidbody` and collider enabled.

---

# README_Basket.md

## 📋 Description
The `Basket` script manages behavior when apples enter the correct basket. This is the counterpart to `BadBasket`, and is meant to register successful interactions.

---

## ⚙️ How It Works
- Detects collisions with good apples.
- Tracks whether the apple is the right type.
- Sends events to scoring systems or Firebase loggers.

---

## 🔗 Dependencies
- `UnityEngine`
- Tag-based apple detection

---

## 🛠️ Usage
1. Attach this script to the **"good" basket** object.
2. Make sure apples are tagged correctly.
3. Integrate with score or data systems if needed.

---

## ✅ Recommended Settings
- Use with a collider marked as **trigger**.
- Apples should use Rigidbody for proper physics.


---
# HandCalibrationManager.md

## 📋 Description
The `HandCalibrationManager` script handles the process of calibrating hand positions and distances during the game session. It helps to ensure personalized hand tracking for each player.

---

## ⚙️ Features
- Calibration of hand reach or stretch.
- Saving calibrated values for session use.
- Potential integration with Firebase for storing user profiles.

---

## 🛠️ Setup
1. Attach to a manager GameObject in the scene.
2. Add UI buttons or voice commands to trigger calibration.
3. Store calibrated values in a central manager or Firebase.

---

## 🔗 Dependencies
- `UnityEngine`
- Optionally: Firebase, TMP, XR Toolkit

---

## 📌 Notes
- Ensure hand models are active when calibration is triggered.
- Save calibration data to use it during gameplay.

---

# GameTimer.md

## 📋 Description
`GameTimer` fetches game duration settings from Firebase Firestore and manages a countdown timer for the game session.

---

## ⚙️ Features
- Retrieves time settings from Firestore (`oyunAyarlari > sure`).
- Displays countdown using `TMP_Text`.
- Reloads scene when time reaches 0.

---

## 🔗 Dependencies
- `Firebase.Firestore`
- `UnityEngine.SceneManagement`
- `TextMeshPro`

---

## 🛠️ Usage
1. Attach to a GameObject in the scene.
2. Assign `countdownText` in the inspector.
3. Link `HastaVeriYoneticisi` to access patient info (`tcKimlikNo`).

---

## 🔄 Firestore Structure
hastalar/{tcKimlikNo}/gunlukKayitlar/{auto_generated_id}
└─ oyunAyarlari: {
sure: number
}

---

# AppleTracker.md

## 📋 Description
`AppleTracker` is a Unity MonoBehaviour script designed to keep track of "apple" interactions (bad/good, correct/incorrect) during gameplay, and store or update the results in **Firebase Firestore** under the corresponding patient's daily logs.

---

## 🎯 Purpose
To maintain accurate daily statistics about how many bad/good apples a patient interacts with and whether their responses were correct — all stored under the `gunlukKayitlar` collection inside a specific patient document (`hastalar/{tcNo}`).

---

## 🔌 Dependencies
- `Firebase.Firestore`
- `System.Threading.Tasks`
- `UnityEngine`
- `System.Linq`, `System.Collections.Generic`

---

## ⚙️ Key Methods

### `void Start()`
Initializes Firestore (`db = FirebaseFirestore.DefaultInstance`).

---

### `void RecordApple(string tcNo, bool isBad, bool isCorrect)`
Tracks whether the interacted apple is bad or good, and whether the user made the correct decision.
- 🔹 If there is an existing daily record, the latest one is updated.
- 🔹 If no record exists, a new one is created using `CreateNewRecord()`.

#### Firestore structure example:
hastalar/{tcNo}/gunlukKayitlar/{auto_generated_id}
├─ totalBadApples: number
├─ totalApples: number
├─ correctBadApples: number
├─ correctApples: number
└─ timestamp: server time

---

### `Task CreateNewRecord(DocumentReference patientRef, bool isBad, bool isCorrect)`
Creates a new log entry in `gunlukKayitlar` with initial counts based on the apple type and correctness.

---

## 🧠 Logic Summary

| Condition                  | Affected Fields                            |
|---------------------------|---------------------------------------------|
| `isBad == true`           | `totalBadApples++`                          |
| `isCorrect == true`       | `correctBadApples++` or `correctApples++`   |
| `isBad == false`          | `totalApples++`                             |

---

## ⚠️ Error Handling
If no valid record exists, or Firestore encounters an error, the script logs detailed messages using `Debug.LogError()` or `Debug.LogWarning()`.

---

## 📝 Notes
- Uses `OrderByDescending("timestamp").Limit(1)` for efficient last-record retrieval.
- Updates only the necessary fields to avoid overwriting unrelated data.
- Adds `timestamp` to each update for logging purposes.
