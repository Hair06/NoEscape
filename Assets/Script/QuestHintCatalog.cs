using UnityEngine;

/// <summary>
/// Nội dung dự phòng cho hệ thống gợi ý thích ứng.
/// Catalog giúp các scene cũ vẫn có gợi ý khu vực/cách làm mà không cần
/// chỉnh trực tiếp file scene. Dữ liệu mới ngoài catalog vẫn dùng SubQuestData.
/// </summary>
public static class QuestHintCatalog
{
    private sealed class HintSet
    {
        public readonly string direction;
        public readonly string location;
        public readonly string action;

        public HintSet(
            string direction,
            string location,
            string action)
        {
            this.direction = direction;
            this.location = location;
            this.action = action;
        }

        public string Get(int level)
        {
            switch (Mathf.Clamp(level, 0, 2))
            {
                case 0:
                    return direction;

                case 1:
                    return location;

                default:
                    return action;
            }
        }
    }

    private static readonly HintSet[][] Hints =
    {
        // Chương mở đầu
        new[]
        {
            new HintSet(
                "Máy phát điện cần hai can xăng trước khi có thể hoạt động.",
                "Một can ở khu nhà kho/sân sau; can còn lại nằm quanh khu máy phát điện ở bên ngoài ngôi nhà.",
                "Rà sát chân tường, thùng đồ và các góc tối của hai khu vực đó. Đến gần từng can rồi nhấn E; bộ đếm phải đạt 2/2."),
            new HintSet(
                "Mang đủ nhiên liệu trở lại máy phát điện.",
                "Máy phát nằm ngoài nhà, tại khu có dây điện và thiết bị cơ khí nối vào căn nhà.",
                "Đứng sát máy phát và nhấn E. Trong bảng đổ xăng, bấm chuột trái liên tục tới khi thanh nhiên liệu đầy, sau đó khởi động máy."),
        },

        // Chương 1
        new[]
        {
            new HintSet(
                "Bốn mảnh ảnh bị xé vẫn còn nằm rải rác trong thư viện.",
                "Chỉ tìm trong thư viện tầng trên: quanh kệ sách, bàn đọc, phía sau đồ nội thất đổ và các góc sát tường.",
                "Soi lần lượt bốn mốc trong thư viện, đưa tâm ngắm vào từng mảnh và nhấn E. Kiểm tra bảng nhiệm vụ cho tới khi đủ 4/4."),
            new HintSet(
                "Bốn mảnh ảnh cần được đưa về chiếc khung còn trống.",
                "Khung tranh trống nằm trong phòng khách, tại khu trưng bày ảnh gia đình.",
                "Sau khi đủ 4/4, đứng trước khung tranh trống và nhấn E để đưa các mảnh vào bảng ghép ảnh."),
            new HintSet(
                "Các đường viền và chi tiết trên ảnh cho biết vị trí của từng mảnh.",
                "Mục tiêu nằm ngay trong bảng ghép ảnh đang mở; không cần tìm thêm vật phẩm ngoài thế giới.",
                "Giữ chuột trái để kéo từng mảnh. Ghép theo khuôn mặt, quần áo, đường viền và nền; thả vào đúng ô cho tới khi cả bốn mảnh khóa vị trí."),
            new HintSet(
                "Bức ảnh hoàn chỉnh đang giải phóng một ký ức bị phong ấn.",
                "Không cần rời đi hoặc tìm vật phẩm mới; đoạn ký ức sẽ phát ngay sau khi ghép xong.",
                "Xem hết cutscene và chờ màn hù sau cutscene kết thúc. Chương kế tiếp chỉ bắt đầu khi quyền điều khiển đã được trả lại."),
        },

        // Chương 2
        new[]
        {
            new HintSet(
                "Con Thoi Nhạc được giấu cùng một ký ức của mẹ.",
                "Tìm ngăn kéo có cuốn nhật ký trong phòng ngủ; Con Thoi nằm sau lớp băng keo của chuỗi tương tác này.",
                "Nhấn E mở ngăn kéo, đọc rồi đóng nhật ký. Mini game gỡ băng keo sẽ tự mở; kéo/gỡ hết băng rồi nhấn E để nhặt Con Thoi được lộ ra."),
            new HintSet(
                "Lò Xo Nhạc có thể nằm gần những vật dụng cơ khí cũ.",
                "Kiểm tra khu bàn sửa chữa, tủ dụng cụ và cụm đồ kim loại trong phần nhà có nhiều máy móc.",
                "Tìm chi tiết kim loại nhỏ hình lò xo, đưa tâm ngắm vào nó rồi nhấn E. Hotbar phải hiện vật phẩm Lò Xo."),
            new HintSet(
                "Chiếc Đĩa Nhạc đã vỡ và phải được ghép lại trước khi sử dụng.",
                "Đĩa vỡ nằm gần cửa sổ; hãy tìm cụm mảnh đĩa trên bề mặt sát vùng có ánh sáng hắt vào.",
                "Nhấn E tại đĩa vỡ để mở mini game, kéo mọi mảnh vào đúng đường viền. Khi bảng đóng, nhấn E thêm một lần tại chiếc đĩa đã sửa để thu thập."),
            new HintSet(
                "Chìa Vặn được cất trong một chiếc hộp có khóa ký hiệu.",
                "Tìm hộp khóa trong phòng và đối chiếu các ký hiệu với manh mối trên nhật ký/lá thư đã xem.",
                "Xoay từng vòng khóa tới đúng thứ tự ký hiệu, mở nắp hộp rồi nhấn E vào Chìa Vặn bên trong để thu thập."),
            new HintSet(
                "Đủ bốn bộ phận rồi; hãy trở lại chiếc hộp nhạc của mẹ.",
                "Hộp nhạc nằm tại khu ký ức nơi giai điệu của mẹ phát ra, không nằm trong các bảng mini game.",
                "Đứng sát hộp nhạc và nhấn E để lắp Con Thoi, Lò Xo, Đĩa Nhạc và Chìa Vặn. Chờ fade/cutscene kết thúc để chuyển chương."),
        },

        // Chương 3
        new[]
        {
            new HintSet(
                "Cánh cửa căn phòng cũ bị đóng bằng các tấm gỗ; cần một dụng cụ để phá chúng.",
                "Tìm xà beng quanh khu nhà kho, sau đó quay lại cánh cửa bị đóng ván trong khu nhà kính cũ.",
                "Đến gần xà beng và nhấn E để nhặt. Mang nó tới cửa, nhấn E để gỡ/phá đủ các tấm gỗ và mở lối vào phòng."),
            new HintSet(
                "Căn phòng vừa mở đang cất một cổ vật hình con mắt.",
                "Đi vào phòng sau cánh cửa đóng ván; kiểm tra kệ đồ và các vật trưng bày sát tường.",
                "Đưa tâm ngắm vào Con Mắt Giáo Phái và nhấn E. Khi nhặt đúng, vật phẩm Con Mắt phải xuất hiện trong hotbar."),
            new HintSet(
                "Con Mắt cho phép nhìn thấy ký tự và dấu chân mà mắt thường bỏ sót.",
                "Cầm Con Mắt trong nhà và quan sát các bề mặt tối; ký tự giáo phái sẽ phát sáng khi tầm nhìn đặc biệt bật.",
                "Chọn Con Mắt, giữ chuột phải để kích hoạt tầm nhìn, lần theo dấu phát sáng tới Ký Tự rồi nhấn E để thu thập."),
            new HintSet(
                "Trái Tim và Giọt Máu được giấu ở hai nhánh khác nhau của mê cung sân trước.",
                "Ra mê cung phía trước nhà. Dùng tầm nhìn Con Mắt để nhận ra hai tuyến dấu chân dẫn tới hai ngõ cụt riêng.",
                "Giữ chuột phải khi cầm Con Mắt, đi hết từng tuyến dấu chân. Nhấn E tại Trái Tim và Giọt Máu; tiến độ phải đạt 2/2."),
            new HintSet(
                "Bàn thờ đang chờ đủ bốn vật phẩm giáo phái.",
                "Quay lại bàn thờ phong ấn trong khu nghi lễ gần căn phòng cũ.",
                "Đứng trước bàn thờ và nhấn E. Có thể đặt từng món; tiếp tục cho tới khi Con Mắt, Ký Tự, Trái Tim và Giọt Máu đều xuất hiện trên bàn thờ."),
        },

        // Chương 4 hiện có trong scene: câu đố hai viên đá và cửa đá.
        new[]
        {
            new HintSet(
                "Cánh cửa đá cần hai viên đá nghi lễ: một xanh và một đỏ.",
                "Tìm quanh khu vườn cũ và lối dẫn tới cụm bệ đá; hai viên nằm ở hai phía khác nhau của khu vực này.",
                "Đến gần từng viên và nhấn E. Hoàn thành mục tiêu khi đã nhặt đủ Đá Xanh và Đá Đỏ (2/2)."),
            new HintSet(
                "Mỗi viên đá phải được đặt lên chiếc bệ cùng màu.",
                "Hai bệ phát sáng nằm ngay trước cánh cửa đá chìm, trong cụm di tích có hai vị trí đặt đá.",
                "Đứng trong vùng của bệ xanh rồi nhấn E; làm tương tự với bệ đỏ. Khi cả hai viên được đặt, cửa đá sẽ tự hạ xuống."),
        },
    };

    public static string GetHint(
        int chapterIndex,
        int subQuestIndex,
        int level,
        SubQuestData fallback)
    {
        if (chapterIndex >= 0 &&
            chapterIndex < Hints.Length &&
            Hints[chapterIndex] != null &&
            subQuestIndex >= 0 &&
            subQuestIndex < Hints[chapterIndex].Length &&
            Hints[chapterIndex][subQuestIndex] != null)
        {
            string catalogHint =
                Hints[chapterIndex][subQuestIndex].Get(level);

            if (!string.IsNullOrWhiteSpace(catalogHint))
            {
                return catalogHint;
            }
        }

        if (fallback == null)
        {
            return "Hãy quan sát lại mục tiêu và các dấu hiệu trong môi trường.";
        }

        switch (Mathf.Clamp(level, 0, 2))
        {
            case 0:
                return fallback.hint;

            case 1:
                return !string.IsNullOrWhiteSpace(fallback.locationHint)
                    ? fallback.locationHint
                    : fallback.hint;

            default:
                if (!string.IsNullOrWhiteSpace(fallback.actionHint))
                {
                    return fallback.actionHint;
                }

                return !string.IsNullOrWhiteSpace(fallback.locationHint)
                    ? fallback.locationHint
                    : fallback.hint;
        }
    }

    public static QuestData CreateChapterFour()
    {
        return new QuestData
        {
            chapterTitle = "CHƯƠNG 4 - CÁNH CỬA ĐÁ",
            characterThought =
                "Nghi lễ đã làm lộ ra một cánh cửa đá. Hai bệ trống trước cửa dường như đang chờ những viên đá cùng màu.",
            characterThoughtDelay = 0.8f,
            characterThoughtHoldTime = 5f,
            mainQuest =
                "Tìm hai viên đá nghi lễ, đặt chúng lên đúng bệ và mở cánh cửa đá.",
            waitForTransitionSignal = false,
            nextChapterStartDelay = 0.8f,
            chapterCompleteMessage =
                "Hai viên đá đã trở về đúng vị trí. Cánh cửa cổ đang hạ xuống...",
            allowOutOfOrderCompletion = true,
            subQuests = new[]
            {
                new SubQuestData
                {
                    title = "Tìm Đá Xanh và Đá Đỏ",
                    hint = Hints[4][0].direction,
                    locationHint = Hints[4][0].location,
                    actionHint = Hints[4][0].action,
                    hintShowDelay = 0.8f,
                    hintHoldTime = -1f,
                    locationHintDelay = 25f,
                    actionHintDelay = 55f,
                },
                new SubQuestData
                {
                    title = "Đặt hai viên đá lên đúng bệ",
                    hint = Hints[4][1].direction,
                    locationHint = Hints[4][1].location,
                    actionHint = Hints[4][1].action,
                    hintShowDelay = 0.8f,
                    hintHoldTime = -1f,
                    locationHintDelay = 20f,
                    actionHintDelay = 45f,
                },
            },
        };
    }
}
