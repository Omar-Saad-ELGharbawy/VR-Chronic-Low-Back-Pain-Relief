import cv2
import mediapipe as mp
import struct
import socket

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

# ################## Functions ####################
def return_bytes(pose_results):
    lst = []
    for i in range(33):
        pt = pose_results.pose_landmarks.landmark[i]
        lst.append(pt.x)
        lst.append(pt.y)
        lst.append(pt.z)
        lst.append(pt.visibility)
    return struct.pack('%sf' % len(lst), *lst)

# ################ Socket Connection ################

# Define server address and port
HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 11000        # The port used by the server

s =  socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# ############## Video Capture ####################

cap = cv2.VideoCapture(0)
while cap.isOpened():
    # read frame
    _, frame = cap.read()
    try:
        with mp_pose.Pose(
            static_image_mode=True,
            model_complexity=0,
            enable_segmentation=True,
            min_detection_confidence=0.5) as pose:

            # resize the frame for portrait video
            frame = cv2.resize(frame, (600,480))
            # convert to RGB
            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            # process the frame for pose detection
            pose_results = pose.process(frame_rgb)

            # print(pose_results.pose_landmarks.landmark[0].x)


            # ############# Send Landmarks ################
            s.sendto(return_bytes(pose_results), (HOST, PORT))

            mp_drawing.draw_landmarks(
                frame,
                pose_results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS
                ) 
            cv2.imshow('Output', frame)
    except:
        break
    if cv2.waitKey(100) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()