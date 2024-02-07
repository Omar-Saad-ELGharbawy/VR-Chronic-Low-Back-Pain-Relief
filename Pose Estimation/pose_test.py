import cv2
import mediapipe as mp

from mediapipe.framework.formats import landmark_pb2



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

        # landmark_subset = landmark_pb2.NormalizedLandmarkList(
        #     landmark = [
        #                 pose_results.pose_landmarks.landmark[11:13],
        #                 pose_results.pose_landmarks.landmark[23:25]
        #                 ])


        # # draw skeleton on the frame
        # mp_drawing.draw_landmarks(frame, landmark_subset)
        # # display the frame
        # cv2.imshow('Output', frame)
    except:
        break

    if cv2.waitKey(100) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()