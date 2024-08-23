using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using RenderHeads.Media.AVProVideo;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Connects a UI slider control to a video player, allowing users to scrub to a particular time in th video.
    /// </summary>
    //[RequireComponent(typeof(VideoPlayer))]
    public class VideoTimeScrubControl : MonoBehaviour
    {
        
        [Tooltip("Video play/pause button GameObject")]
        public GameObject m_ButtonPlayOrPause;

        [Tooltip("Slider that controls the video")]
        public Slider m_Slider;

        [Tooltip("Play icon sprite")]
        public Sprite m_IconPlay;

        [Tooltip("Pause icon sprite")]
        public Sprite m_IconPause;

        [Tooltip("Play or pause button image.")]
        public Image m_ButtonPlayOrPauseIcon;
        
        [Tooltip("Text that displays the current time of the video.")]
        public TextMeshProUGUI m_VideoTimeText;

        [Tooltip("If checked, the slider will fade off after a few seconds. If unchecked, the slider will remain on.")]
        public bool m_HideSliderAfterFewSeconds;

        bool m_IsDragging;
        bool m_VideoIsPlaying;
        bool m_VideoJumpPending;
        long m_LastFrameBeforeScrub;
        MediaPlayer m_VideoPlayer;

        void Start()
        {
            m_VideoPlayer = GetComponent<MediaPlayer>();
            

            m_VideoPlayer.AutoStart = false; // Set play on awake for next enable.
            VideoPlay(); // Play video to load first frame.
            VideoStop(); // Stop the video to set correct state and pause frame.
            

            if (m_ButtonPlayOrPause != null)
                m_ButtonPlayOrPause.SetActive(false);
        }

        public void Initialize()
        {
            m_VideoPlayer = GetComponent<MediaPlayer>();
        }

        public void OnEnable()
        {
            
            if (m_VideoPlayer != null)
            {
                
                m_VideoPlayer.Control.Seek(0);
                VideoPlay(); // Ensures correct UI state update if paused.
            }

            if (m_Slider != null)
            {
                m_Slider.value = 0.0f;
                m_Slider.onValueChanged.AddListener(OnSliderValueChange);
                m_Slider.gameObject.SetActive(true);
                if (m_HideSliderAfterFewSeconds)
                    StartCoroutine(HideSliderAfterSeconds());
            }
            
        }

        void Update()
        {
            
            if (m_VideoJumpPending)
            {
                // We're trying to jump to a new position, but we're checking to make sure the video player is updated to our new jump frame.
                if (m_LastFrameBeforeScrub == m_VideoPlayer.Control.GetCurrentTimeFrames())
                    return;

                // If the video player has been updated with desired jump frame, reset these values.
                m_LastFrameBeforeScrub = long.MinValue;
                m_VideoJumpPending = false;
            }

            if (!m_IsDragging && !m_VideoJumpPending)
            {
                if (m_VideoPlayer.Info.GetDurationFrames() > 0)
                {
                    var progress = (float)m_VideoPlayer.Control.GetCurrentTimeFrames() / m_VideoPlayer.Info.GetDurationFrames();
                    m_Slider.value = progress;
                }
            }
            
        }

        public void OnPointerDown()
        {
            Debug.Log("On Pointer Down");
            m_VideoJumpPending = true;
            VideoStop();
            VideoJump();
        }

        public void OnRelease()
        {
           // Debug.Log("On Release");
            m_IsDragging = false;
            VideoPlay();
            VideoJump();
        }

        void OnSliderValueChange(float sliderValue)
        {
            UpdateVideoTimeText();
        }

        IEnumerator HideSliderAfterSeconds(float duration = 1f)
        {
            yield return new WaitForSeconds(duration);
            m_Slider.gameObject.SetActive(false);
        }

        public void OnDrag()
        {
            //Debug.Log("On Drag");
            m_IsDragging = true;
            m_VideoJumpPending = true;
        }

        void VideoJump()
        {
            
            m_VideoJumpPending = true;
            var frame = m_VideoPlayer.Info.GetDurationFrames() * m_Slider.value;
            m_LastFrameBeforeScrub = m_VideoPlayer.Control.GetCurrentTimeFrames();
            m_VideoPlayer.Control.SeekToFrame((int)frame);
            
        }

        public void PlayOrPauseVideo()
        {
            if (m_VideoIsPlaying)
            {
                VideoStop();
            }
            else
            {
                VideoPlay();
            }
        }

        void UpdateVideoTimeText()
        {
            if (m_VideoPlayer != null && m_VideoTimeText != null)
            {

                var currentTimeTimeSpan = TimeSpan.FromSeconds(m_VideoPlayer.Control.GetCurrentTime());
                var totalTimeTimeSpan = TimeSpan.FromSeconds(m_VideoPlayer.Info.GetDuration());
                var currentTimeString = string.Format("{0:D2}:{1:D2}",
                    currentTimeTimeSpan.Minutes,
                    currentTimeTimeSpan.Seconds
                );

                var totalTimeString = string.Format("{0:D2}:{1:D2}",
                    totalTimeTimeSpan.Minutes,
                    totalTimeTimeSpan.Seconds
                );

                m_VideoTimeText.SetText(currentTimeString + " / " + totalTimeString);
            } else
            {
                
            }
        }

        public void VideoStop()
        {

            // Debug.Log("Pausing Video!");
            m_VideoIsPlaying = false;
            m_VideoPlayer.Control.Pause();
            m_ButtonPlayOrPauseIcon.sprite = m_IconPlay;
            m_ButtonPlayOrPause.SetActive(true);
        }

        public void VideoPlay()
        {
           // Debug.Log("Playing Video!");
            m_VideoIsPlaying = true;
            m_VideoPlayer.Control.Play();
            m_ButtonPlayOrPauseIcon.sprite = m_IconPause;
            m_ButtonPlayOrPause.SetActive(false); 
        }
    }
}
