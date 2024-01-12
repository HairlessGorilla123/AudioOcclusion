using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioOcclusion : MonoBehaviour
{
    // Audio listener to calculate distance
    public AudioListener audioListener;

    // Volume falloff curve based on distance
    public AnimationCurve audioFalloff = AnimationCurve.Linear(0, 1, 1, 0);

    // Threshold for dampening audio on intersection
    [Range(0f, 1f)]
    [Tooltip("The amount of dampening the audio will experience for each intersection")]
    public float dampenThreshold = 0.1f;

    // Maximum range the audio will travel without any collisions
    [Tooltip("The maximum range the audio will travel without any collisions")]
    public float maximumRange = 25f;

    // Smoothing factor for volume transitions
    [Space]
    [Tooltip("How smoothly the volume will transition from one level to another")]
    public float volumeSmoothener = 3f;

    // Optional reverb filter effects
    [Header("Reverb Filter Effects (Optional)")]
    [Tooltip("Leave null for no effects")]
    public AudioReverbFilter audioReverbFilter;

    // Threshold for reverb intensity on intersection
    [Range(0f, 1f)]
    [Tooltip("The change in intensity of the reverb that the audio will experience for each intersection")]
    public float reverbThreshold = 0.1f;

    // Reference to the AudioSource component
    private AudioSource audioSource;

    private void Start()
    {
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();

        // Perform initial sound occlusion calculation
        PerformSoundOcclusion(1);

        // Disable built-in audio rolloff
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 0;
        audioSource.maxDistance = 0;
    }

    private void Update()
    {
        // Update sound occlusion calculations each frame
        PerformSoundOcclusion(Time.deltaTime);
    }

    private void PerformSoundOcclusion(float deltaTime)
    {
        // Calculate distance from audio source to audio listener
        float distanceFromSource = Vector3.Distance(transform.position, audioListener.transform.position);

        // Perform raycast to detect obstacles
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, AngleToAudioListener(), distanceFromSource);

        // Calculate normalized falloff value based on distance
        float falloffValue = audioFalloff.Evaluate(distanceFromSource / maximumRange);

        // Adjust source volume based on raycast hits and falloff value
        float sourceVolume = Mathf.Pow(dampenThreshold, hits.Length - 1);
        sourceVolume *= falloffValue;
        audioSource.volume = Mathf.Lerp(audioSource.volume, sourceVolume, volumeSmoothener * deltaTime);

        // Reverb effects
        if (audioReverbFilter != null)
        {
            // Adjust reverb intensity based on raycast hits
            float reverbIntensity = Mathf.Pow(reverbThreshold, (hits.Length - 1));
            reverbIntensity = 1 - reverbIntensity;
            reverbIntensity = reverbIntensity * 12000 - 10000;
            reverbIntensity = Mathf.Clamp(reverbIntensity, -10000, 2000);

            // Set reverb preset to user-customizable
            audioReverbFilter.reverbPreset = AudioReverbPreset.User;

            // Set reverbLevel value (intensity of reverb effects)
            audioReverbFilter.reverbLevel = Mathf.Lerp(audioReverbFilter.reverbLevel, reverbIntensity, volumeSmoothener * deltaTime);
        }
    }

    // Calculate the direction vector from audio source to audio listener
    private Vector3 AngleToAudioListener()
    {
        Vector3 direction = audioListener.transform.position - transform.position;
        direction.Normalize();
        return direction;
    }

    // Visualize ray in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, AngleToAudioListener());
        Gizmos.DrawWireSphere(transform.position, maximumRange);
    }
}
