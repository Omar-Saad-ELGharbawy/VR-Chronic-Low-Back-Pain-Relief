import cv2
import mediapipe as mp


## initialize pose estimator
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
pose = mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5, model_complexity = 0)

cap = cv2.VideoCapture(0)
while cap.isOpened():
    # read frame
    _, frame = cap.read()
    try:
        # resize the frame for portrait video
        frame = cv2.resize(frame, (600,480))
        # convert to RGB
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        # process the frame for pose detection
        pose_results = pose.process(frame_rgb)

        # Get landmark 23 (right HIP)
        right_hip_landmark = pose_results.pose_landmarks.landmark[23]
        # Get landmark 24 (lift HIP)
        left_hip_landmark = pose_results.pose_landmarks.landmark[24]
        # Get landmark 11 (right Shoulder)
        right_shoulder_landmark = pose_results.pose_landmarks.landmark[11]

        # Calculate slope
        # x1, y1 = left_hip_landmark.x * frame.shape[1], left_hip_landmark.y * frame.shape[0]
        # x2, y2 = right_hip_landmark.x * frame.shape[1], right_hip_landmark.y * frame.shape[0]
        x1, y1 = left_hip_landmark.x , left_hip_landmark.y 
        x2, y2 = right_hip_landmark.x , right_hip_landmark.y 

        
        if x2 - x1 != 0:  # Avoid division by zero
            slope = (y2 - y1) / (x2 - x1)
            print("Slope between right HIP (point 23) and lift HIP (point 24):", slope)

        # Calculate slope in z direction
        z_difference = right_shoulder_landmark.z - right_hip_landmark.z
        y_difference = right_shoulder_landmark.y - right_hip_landmark.y
        if y_difference != 0: 
            slope_z = z_difference / y_difference
            print("Slope in z direction:", slope_z)


        # draw skeleton on the frame
        # mp_drawing.draw_landmarks(frame, pose_results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
        # display the frame
        cv2.imshow('Output', frame)
    except:
        break

    if cv2.waitKey(100) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()