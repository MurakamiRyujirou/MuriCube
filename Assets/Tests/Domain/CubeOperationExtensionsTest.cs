using Domain.Cube.Enums;
using NUnit.Framework;
using Presentation;

[TestFixture]
public class CubeOperationExtensionsTest
{
    [Test] public void R_Returns_X_Clockwise() => Assert.AreEqual((RotateAxis.X, CubeTurn.Clockwise), CubeOperation.R.ToAxisAndTurn());
    [Test] public void Ri_Returns_X_CounterClockwise() => Assert.AreEqual((RotateAxis.X, CubeTurn.CounterClockwise), CubeOperation.Ri.ToAxisAndTurn());
    [Test] public void L_Returns_X_CounterClockwise() => Assert.AreEqual((RotateAxis.X, CubeTurn.CounterClockwise), CubeOperation.L.ToAxisAndTurn());
    [Test] public void Li_Returns_X_Clockwise() => Assert.AreEqual((RotateAxis.X, CubeTurn.Clockwise), CubeOperation.Li.ToAxisAndTurn());
    [Test] public void U_Returns_Y_Clockwise() => Assert.AreEqual((RotateAxis.Y, CubeTurn.Clockwise), CubeOperation.U.ToAxisAndTurn());
    [Test] public void Ui_Returns_Y_CounterClockwise() => Assert.AreEqual((RotateAxis.Y, CubeTurn.CounterClockwise), CubeOperation.Ui.ToAxisAndTurn());
    [Test] public void D_Returns_Y_CounterClockwise() => Assert.AreEqual((RotateAxis.Y, CubeTurn.CounterClockwise), CubeOperation.D.ToAxisAndTurn());
    [Test] public void Di_Returns_Y_Clockwise() => Assert.AreEqual((RotateAxis.Y, CubeTurn.Clockwise), CubeOperation.Di.ToAxisAndTurn());
    [Test] public void F_Returns_Z_Clockwise() => Assert.AreEqual((RotateAxis.Z, CubeTurn.Clockwise), CubeOperation.F.ToAxisAndTurn());
    [Test] public void Fi_Returns_Z_CounterClockwise() => Assert.AreEqual((RotateAxis.Z, CubeTurn.CounterClockwise), CubeOperation.Fi.ToAxisAndTurn());
    [Test] public void B_Returns_Z_CounterClockwise() => Assert.AreEqual((RotateAxis.Z, CubeTurn.CounterClockwise), CubeOperation.B.ToAxisAndTurn());
    [Test] public void Bi_Returns_Z_Clockwise() => Assert.AreEqual((RotateAxis.Z, CubeTurn.Clockwise), CubeOperation.Bi.ToAxisAndTurn());
}
