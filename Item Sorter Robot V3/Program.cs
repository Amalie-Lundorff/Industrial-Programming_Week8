using System.Net.Sockets;
using System.Text;

const string program = @"
def main():
  # --- Gripper stub (ingen XML-RPC i sim) ---
  def rg_is_busy():
    return False
  end

  # width: 0..110 (mm), force: 0..40
  def rg_grip(width, force=10):
    # Simuler et gripper-kald uden netværk:
    sleep(0.05)
  end

  # --- Pick & Place helper (uden get_inverse_kin) ---
  def pick_and_place(p_pick, p_place, dz=0.2, a=1.0, v=0.5, open_mm=80, close_mm=0, f_open=20, f_close=30):
    p_pick_up  = pose_trans(p_pick,  p[0,0,dz,0,0,0])
    p_place_up = pose_trans(p_place, p[0,0,dz,0,0,0])

    # Åbn før indkørsel
    rg_grip(open_mm, f_open)

    # --- PICK ---
    movej(p_pick_up, a, v)
    movel(p_pick, a, v)
    rg_grip(close_mm, f_close)
    sleep(0.1)
    movel(p_pick_up, a, v)

    # --- PLACE ---
    movej(p_place_up, a, v)
    movel(p_place, a, v)
    rg_grip(open_mm, f_open)
    sleep(0.1)
    movel(p_place_up, a, v)
  end

  # --- Jeres målte TCP-poser (mm -> meter) ---
  p_pos1 = p[0.25,   -0.25,   0, 0.128,  3.085, -0.054]
  p_pos2 = p[0.01880,-0.26657,0, 0.060, -3.096, -0.050]
  p_pos3 = p[0.02150,-0.42928,0, 0.125, -3.143,  0.037]
  p_pos4 = p[0.17509,-0.42709,0, 0.080,  3.106,  0.035]

  # Fælles parametre
  dz = 0.12
  a  = 1.0
  v  = 0.5
  open_mm  = 60
  close_mm = 15
  f_open   = 20
  f_close  = 30

  # --- 1) pos1 -> pos2 ---
  pick_and_place(p_pos1, p_pos2, dz, a, v, open_mm, close_mm, f_open, f_close)
  sleep(0.2)
  # --- 2) pos3 -> pos4 ---
  pick_and_place(p_pos3, p_pos4, dz, a, v, open_mm, close_mm, f_open, f_close)
end

main()
";

const int urscriptPort = 30002, dashboardPort = 29999;
const string IpAddress = "127.0.0.1"; // URSim i Docker på din PC

void SendString(string host, int port, string message)
{
   using var client = new TcpClient(host, port);
   using var stream = client.GetStream();
   stream.Write(Encoding.ASCII.GetBytes(message));
}

// (valgfrit) prøv at frigive bremser via dashboard:
SendString(IpAddress, dashboardPort, "brake release\n");

// send URScript til secondary interface:
SendString(IpAddress, urscriptPort, program);

// (valgfrit) stop programmet:
// SendString(IpAddress, dashboardPort, "stop\n");
